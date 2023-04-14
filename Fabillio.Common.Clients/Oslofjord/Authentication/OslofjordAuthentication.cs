using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Fabillio.Common.Clients.Oslofjord.Configurations;
using Fabillio.Common.Clients.Oslofjord.Models.Responses.Authentication;

namespace Fabillio.Common.Clients.Oslofjord.Authentication;

public class OslofjordAuthentication
{
    private readonly OslofjordOptions _oslofjordOptions;

    public OslofjordAuthentication(OslofjordOptions oslofjordOptions)
    {
        _oslofjordOptions = oslofjordOptions;
    }

    protected static ConcurrentDictionary<
        string,
        (DateTimeOffset expiry, TokenResponse tokenResponse)
    > _tokens =
        new ConcurrentDictionary<string, (DateTimeOffset expiry, TokenResponse tokenResponse)>();
    protected static ConcurrentDictionary<string, SemaphoreSlim> _semaphores =
        new ConcurrentDictionary<string, SemaphoreSlim>();

    public async Task<TokenResponse> RequestTokenAsync()
    {
        // Retrieve cached token
        var tokenKey = $"{_oslofjordOptions.Auth.Authority}|{_oslofjordOptions.Auth.ClientId}";

        if (
            _tokens.TryGetValue(
                tokenKey,
                out (DateTimeOffset expiry, TokenResponse tokenResponse) token
            )
            && token.expiry > DateTimeOffset.Now
        )
        {
            return new TokenResponse(
                AccessToken: token.tokenResponse.AccessToken,
                ExpiresIn: token.tokenResponse.ExpiresIn,
                TokenType: token.tokenResponse.TokenType,
                Scope: token.tokenResponse.Scope
            );
        }

        // Ensure only one token request is made at a time
        var requestLock = _semaphores.GetOrAdd(tokenKey, new SemaphoreSlim(1));
        await requestLock.WaitAsync();

        try
        {
            // Check if token has already been retrieved by another thread
            if (_tokens.TryGetValue(tokenKey, out token) && token.expiry > DateTimeOffset.Now)
            {
                return new TokenResponse(
                    AccessToken: token.tokenResponse.AccessToken,
                    ExpiresIn: token.tokenResponse.ExpiresIn,
                    TokenType: token.tokenResponse.TokenType,
                    Scope: token.tokenResponse.Scope
                );
            }

            string content = string.Empty;

            using (HttpClient client = new HttpClient())
            {
                var httpContent = new FormUrlEncodedContent(
                    new Dictionary<string, string>
                    {
                        { "grant_type", "client_credentials" },
                        { "client_id", _oslofjordOptions.Auth.ClientId },
                        { "client_secret", _oslofjordOptions.Auth.Secret },
                        {
                            "scope",
                            "oslofjord-samvirk-api-public.read oslofjord-samvirk-api-public.write oslofjord-samvirk-api-public.refund"
                        }
                    }.ToList()
                );

                var httpMessage = await client.PostAsync(
                    _oslofjordOptions.Auth.Authority,
                    httpContent
                );

                content = await httpMessage.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<TokenResponse>(content);

                if (!string.IsNullOrEmpty(result.AccessToken) && result.AccessToken.Contains('.'))
                {
                    // Determine token expiry
                    var base64Payload = result.AccessToken.Split('.')[1];

                    // Add padding to base64 encoded string
                    for (var i = 0; base64Payload.Length % 4 != 0; i++)
                    {
                        base64Payload += "=";
                    }

                    var payload = JsonConvert.DeserializeAnonymousType(
                        Encoding.UTF8.GetString(Convert.FromBase64String(base64Payload)),
                        new { exp = 0 }
                    );

                    var expiry = DateTimeOffset.FromUnixTimeSeconds(payload.exp);

                    // 10% time buffer
                    var buffer = TimeSpan.FromSeconds(
                        (expiry - DateTimeOffset.Now).TotalSeconds / 10
                    );
                    expiry = expiry - buffer;

                    if (expiry > DateTimeOffset.Now)
                    {
                        // Save token to cache
                        _tokens[tokenKey] = (expiry, result);

                        return new TokenResponse(
                            AccessToken: result.AccessToken,
                            result.ExpiresIn,
                            TokenType: result.TokenType,
                            Scope: result.Scope
                        );
                    }
                }

                return new TokenResponse(
                    AccessToken: default,
                    default,
                    TokenType: default,
                    Scope: default
                );
            }
        }
        finally
        {
            requestLock.Release();
        }
    }
}
