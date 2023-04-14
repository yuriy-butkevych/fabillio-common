using Raven.Client.Documents;
using Raven.TestDriver;

namespace Fabillio.Common.Tests;

public class RavenDbTests : RavenTestDriver
{
    protected override void PreInitialize(IDocumentStore documentStore)
    {
        documentStore.Conventions.MaxNumberOfRequestsPerSession = 30;
    }

    protected virtual void ExecuteIndices(IDocumentStore store) { }
}
