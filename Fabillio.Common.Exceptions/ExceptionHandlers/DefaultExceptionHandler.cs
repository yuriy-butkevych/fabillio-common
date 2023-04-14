using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fabillio.Common.Exceptions.ExceptionHandlers;

internal class DefaultExceptionHandler : IExceptionHandler<Exception>
{
    private readonly ILogger<DefaultExceptionHandler> _logger;

    public DefaultExceptionHandler(ILogger<DefaultExceptionHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleException(Exception exception, HttpContext context)
    {
        string jsonResult = JsonConvert.SerializeObject(
            new { Warnings = new[] { exception.Message } }
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        _logger.LogError(exception, "An error occurred");

        return context.Response.WriteAsync(jsonResult);
    }
}
