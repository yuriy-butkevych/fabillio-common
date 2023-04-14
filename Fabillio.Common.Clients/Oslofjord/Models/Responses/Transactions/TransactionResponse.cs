using System;
using Newtonsoft.Json;

namespace Fabillio.Common.Clients.Oslofjord.Models.Responses.Transactions;

public record TransactionResponse(
    [property: JsonProperty("transactionGuid")] Guid TransactionGuid,
    [property: JsonProperty("mifareId")] string MifareId,
    [property: JsonProperty("timestamp")] DateTime Timestamp,
    [property: JsonProperty("personKey")] string PersonKey,
    [property: JsonProperty("paymentMethod")] string PaymentMethod,
    [property: JsonProperty("amount")] decimal Amount
);
