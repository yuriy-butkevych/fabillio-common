using Newtonsoft.Json;

namespace Fabillio.Common.Clients.Oslofjord.Models.Responses.Cards;

public record CardDetailsResponse(
    [property: JsonProperty("hasGiftCard")] bool HasGiftCard,
    [property: JsonProperty("balance")] decimal Balance,
    [property: JsonProperty("personKey")] string PersonKey
);
