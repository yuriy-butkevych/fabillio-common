namespace Fabillio.Common.Services.Options;

public class BackgroundServiceOptions
{
    public int MinutesToProcessBackgroundCommand { get; set; } = 20;
    public int LogEntitiesExpirationDays { get; set; } = 7;
}
