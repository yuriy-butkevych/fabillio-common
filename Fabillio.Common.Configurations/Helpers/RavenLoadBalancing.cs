using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Fabillio.Common.Configurations.Helpers;

public class RavenLoadBalancing
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RavenLoadBalancing(IHttpContextAccessor httpContextAccessor)
    {
        this._httpContextAccessor = httpContextAccessor;
    }

    public string SessionContextSelector
    {
        get
        {
            var user = this._httpContextAccessor?.HttpContext?.User;
            return user?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                ?? "static";
        }
    }

    public void EnsureInstantiated() { }
}
