using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using Raven.Client.Documents.Operations;
using Raven.Client.Documents.Operations.ConnectionStrings;
using Raven.Client.Documents.Operations.ETL;
using Raven.Client.Documents.Operations.Indexes;
using Raven.Client.Exceptions.Database;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using Raven.DependencyInjection;
using Fabillio.Common.Configurations.DocumentNotifications;
using Fabillio.Common.Configurations.Entities;
using Fabillio.Common.Configurations.Helpers;
using Fabillio.Common.Configurations.Interfaces;
using Fabillio.Common.Configurations.Sections;

namespace Fabillio.Common.Configurations.Extensions;

public static class RavenDbConfiguration
{
    public static IServiceCollection ConfigureRaven(
        this IServiceCollection services,
        string sectionName
    )
    {
        RavenLoadBalancing ravenLoadBalancing = null;

        services.AddSingleton(sp =>
        {
            // We want each user / session to stick to one node within the Raven cluster
            // We need this initialization workaround to get hold of the http context within the BeforeInitializeDocStore delegate
            // see https://ravendb.net/docs/article-page/5.2/Csharp/client-api/session/configuration/use-session-context-for-load-balancing

            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            return ravenLoadBalancing = new RavenLoadBalancing(httpContextAccessor);
        });

        // https://github.com/JudahGabriel/RavenDB.DependencyInjection
        services.AddRavenDbDocStore(options =>
        {
            options.SectionName = sectionName;
        });

        services.AddRavenDbDocStore(options =>
        {
            options.BeforeInitializeDocStore = store =>
            {
                store.Conventions.LoadBalancerPerSessionContextSelector = db =>
                    ravenLoadBalancing?.SessionContextSelector;
            };
            if (!string.IsNullOrWhiteSpace(options.Settings?.CertFilePath))
            {
                options.Certificate = new X509Certificate2(
                    Convert.FromBase64String(options.Settings.CertFilePath),
                    options.Settings.CertPassword
                );
            }
        });

        services.AddRavenDbAsyncSession();
        services.AddRavenDbSession();

        services.AddTransient<IDocumentStoreListener, DocumentStoreListener>();

        return services;
    }

    public static IApplicationBuilder InitRavenDatabase<EntityToEnsureCreated>(
        this IApplicationBuilder app,
        IConfiguration configuration,
        Assembly indexAssembly = null,
        string[] discoveryUrls = null,
        Assembly etlAssembly = null,
        bool? deleteIndexesBeforUpdate = false,
        bool? etlInternalDeletion = false
    ) where EntityToEnsureCreated : class
    {
        var docStore = app.ApplicationServices.GetRequiredService<IDocumentStore>();
        app.ApplicationServices.GetRequiredService<RavenLoadBalancing>().EnsureInstantiated();

        var dbConnections = configuration
            .GetSection("RavenDatabaseConnections")
            .Get<RavenDatabaseConnectInfo[]>();

        if (dbConnections is not null)
        {
            foreach (var connection in dbConnections)
            {
                var ravenConnectionString = new RavenConnectionString()
                {
                    Name = connection.Key,
                    TopologyDiscoveryUrls = discoveryUrls,
                    Database = connection.Value
                };

                docStore.Maintenance.Send(
                    new PutConnectionStringOperation<RavenConnectionString>(ravenConnectionString)
                );
            }
        }

        List<IEtlOperation> etlOperations = new List<IEtlOperation>();

        if (etlAssembly is not null)
        {
            foreach (
                var operation in etlAssembly
                    .GetTypes()
                    .Where(mytype => mytype.GetInterfaces().Contains(typeof(IEtlOperation)))
                    .ToList()
            )
            {
                etlOperations.Add(Activator.CreateInstance(operation) as IEtlOperation);
            }
        }

        if (etlOperations.Count > 0 && dbConnections is not null)
        {
            foreach (var operation in etlOperations)
            {
                foreach (var collection in operation.ArtificialCollections)
                {
                    var databaseConnection = dbConnections.FirstOrDefault(
                        x => x.Key == operation.DestinationDbKey && x.CertificatePath is not null
                    );

                    if (databaseConnection is not null)
                    {
                        if (etlInternalDeletion.GetValueOrDefault())
                        {
                            var certificate = new X509Certificate2(
                                Convert.FromBase64String(
                                    configuration[databaseConnection.CertificatePath]
                                ),
                                configuration[databaseConnection.PasswordPath]
                            );

                            using (
                                IDocumentStore store = new DocumentStore()
                                {
                                    Urls = discoveryUrls,
                                    Conventions =
                                    {
                                        MaxNumberOfRequestsPerSession = 10,
                                        UseOptimisticConcurrency = true
                                    },
                                    Database = databaseConnection.Value,
                                    Certificate = certificate,
                                }.Initialize()
                            )
                            {
                                store.Operations
                                    .ForDatabase(databaseConnection.Value)
                                    .Send(new DeleteByQueryOperation($"from {collection}"))
                                    .WaitForCompletion();
                            }
                        }

                        docStore.Operations
                            .Send(new DeleteByQueryOperation($"from {collection}"))
                            .WaitForCompletion();
                    }
                }
            }
        }

        if (indexAssembly is not null)
        {
            if (deleteIndexesBeforUpdate.GetValueOrDefault())
            {
                var indexes = indexAssembly.ExportedTypes
                    .Where(x => x.Name.EndsWith("Index"))
                    .Select(x => x.Name)
                    .ToList();

                foreach (var index in indexes)
                {
                    docStore.Maintenance.Send(new DeleteIndexOperation(index));
                }
            }

            IndexCreation.CreateIndexes(indexAssembly, docStore);
        }

        var documentStoreListener =
            app.ApplicationServices.GetRequiredService<IDocumentStoreListener>();
        docStore.OnAfterSaveChanges += documentStoreListener.OnAfterRavenDbSaveChanges;
        docStore.OnBeforeStore += documentStoreListener.OnBeforeStore;
        docStore.OnBeforeDelete += documentStoreListener.OnBeforeDelete;

        try
        {
            using var dbSession = docStore.OpenSession();
            _ = dbSession.Query<EntityToEnsureCreated>().Take(0).ToList();
        }
        catch (DatabaseDoesNotExistException)
        {
            docStore.Maintenance.Server.Send(
                new CreateDatabaseOperation(new DatabaseRecord { DatabaseName = docStore.Database })
            );
        }

        foreach (var instance in etlOperations)
        {
            var instanceConfiguration = instance.Configuration;

            if (
                dbConnections is not null
                && !dbConnections.Where(x => x.Key == instance.DestinationDbKey).Any()
            )
                continue;

            instanceConfiguration.ConnectionStringName = instance.DestinationDbKey;

            using var session = docStore.OpenSession();
            var etlOperation = session.Load<EtlOperation>(
                EtlOperation.GetDocumentId(instance.Name)
            );

            if (etlOperation is null)
            {
                var result = docStore.Maintenance.Send(
                    new AddEtlOperation<RavenConnectionString>(instanceConfiguration)
                );

                etlOperation = new EtlOperation();
                etlOperation.Create(instance.Name, result.TaskId);

                session.Store(etlOperation);
            }
            else
            {
                var result = docStore.Maintenance.Send(
                    new UpdateEtlOperation<RavenConnectionString>(
                        etlOperation.TaskId,
                        instanceConfiguration
                    )
                );
                etlOperation.Update(result.TaskId);
            }

            session.SaveChanges();
        }

        return app;
    }
}
