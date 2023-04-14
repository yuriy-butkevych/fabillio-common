namespace Fabillio.Common.Clients.Oslofjord.Models.Responses.Contracts;

public class ContractResponse
{
    public string ContractKey { get; set; }

    public bool? IsBlocked { get; set; }

    public bool? Deleted { get; set; }
}
