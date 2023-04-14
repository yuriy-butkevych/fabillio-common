namespace Fabillio.Common.Clients.Oslofjord.Configurations;

public class OslofjordOptions
{
    public string ApiBaseUrl { get; set; }

    public Auth Auth { get; set; }
}

public class Auth
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
    public string Secret { get; set; }
}
