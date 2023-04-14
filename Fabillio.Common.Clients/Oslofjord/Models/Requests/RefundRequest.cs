using System;

namespace Fabillio.Common.Clients.Oslofjord.Models.Requests;

public class RefundRequest
{
    public string OrderNumber { get; set; }

    public decimal RefundAmount { get; set; }

    public Guid RefundOperationId { get; set; } = Guid.NewGuid();
}
