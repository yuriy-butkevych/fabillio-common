using System.Collections.Generic;
using Raven.Client.Documents.Operations.ETL;

namespace Fabillio.Common.Configurations.Interfaces;

public interface IEtlOperation
{
    string Name { get; }
    RavenEtlConfiguration Configuration { get; }
    string DestinationDbKey { get; }
    public List<string> ArtificialCollections { get; }
}
