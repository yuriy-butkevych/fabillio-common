using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Fabillio.Common.Exceptions.Exceptions;

namespace Fabillio.Common.Helpers.Extensions;

public static class LoggerExtensions
{
    public static void LogInvalidOperation(this ILogger logger, object request, string operation)
    {
        string json = string.Empty;

        if (request is not null)
            json = JsonConvert.SerializeObject(request);

        string message = string.IsNullOrWhiteSpace(json) ? "EMPTY_REQUEST" : json;

        logger.LogError(
            exception: new InvalidSamvirkOperationException(operation),
            message: message
        );
    }
}
