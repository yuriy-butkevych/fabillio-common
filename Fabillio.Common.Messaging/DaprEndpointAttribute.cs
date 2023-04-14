using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Fabillio.Common.Messaging;

public class DaprEndpointAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Result != null)
            return;

        if (!IsLocalIpAddress(context.HttpContext.Request.Host.Host))
        {
            context.Result = new ObjectResult("Not allowed")
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    private static bool IsLocalIpAddress(string host)
    {
        try
        {
            // get host IP addresses
            var hostIPs = Dns.GetHostAddresses(host);
            // get local IP addresses
            var localIPs = Dns.GetHostAddresses(Dns.GetHostName());

            // test if any host IP equals to any local IP or to localhost
            foreach (var hostIp in hostIPs)
            {
                // is localhost
                if (IPAddress.IsLoopback(hostIp))
                {
                    return true;
                }

                // is local address
                if (localIPs.Contains(hostIp))
                {
                    return true;
                }
            }
        }
        catch
        {
            return false;
        }

        return false;
    }
}
