using System;
using Newtonsoft.Json;

namespace Fabillio.Common.Clients.Oslofjord.Models.Requests.Transactions;

public record TransactionQueryRequest(
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] string PersonKey = null,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        DateTime? FromDate = null,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)] DateTime? ToDate = null,
    [property: JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        DateTime? ModifiedAfter = null
);
