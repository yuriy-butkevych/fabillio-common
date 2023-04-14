using System;

namespace Fabillio.Common.Configurations.Entities;

public class EtlOperation
{
    public static string GetDocumentId(string name)
    {
        return "etlOperation/" + name;
    }

    public string Id => GetDocumentId(Name);
    public string Name { get; private set; }
    public long TaskId { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Updated { get; private set; }

    public void Create(string name, long taskId)
    {
        Created = DateTime.UtcNow;
        TaskId = taskId;
        Name = name;
    }

    public void Update(long taskId)
    {
        Updated = DateTime.UtcNow;
        TaskId = taskId;
    }
}
