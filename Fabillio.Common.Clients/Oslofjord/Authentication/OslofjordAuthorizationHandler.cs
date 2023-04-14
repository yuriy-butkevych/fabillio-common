using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Fabillio.Common.Clients.Oslofjord.Authentication;

internal class OslofjordAuthorizationHandler : DelegatingHandler
{
    private readonly OslofjordAuthentication _authTokenStore;

    public OslofjordAuthorizationHandler(OslofjordAuthentication authTokenStore)
    {
        _authTokenStore = authTokenStore;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var tokenResponse = await _authTokenStore.RequestTokenAsync();

        request.Headers.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            tokenResponse.AccessToken
        );

        return await base.SendAsync(request, cancellationToken);
    }
}
