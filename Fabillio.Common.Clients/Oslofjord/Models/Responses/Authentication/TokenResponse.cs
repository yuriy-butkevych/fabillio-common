using Newtonsoft.Json;

namespace Fabillio.Common.Clients.Oslofjord.Models.Responses.Authentication;

public record TokenResponse(
    [property: JsonProperty("access_token")] string AccessToken,
    [property: JsonProperty("expires_in")] int ExpiresIn,
    [property: JsonProperty("token_type")] string TokenType,
    [property: JsonProperty("scope")] string Scope
);
