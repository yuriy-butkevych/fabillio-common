using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Fabillio.Common.Exceptions.ExceptionHandlers;

internal class ValidationExceptionHandler : IExceptionHandler<ValidationException>
{
    public Task HandleException(ValidationException exception, HttpContext context)
    {
        string jsonResult = JsonConvert.SerializeObject(
            new { Warnings = exception.Errors.Select(x => x.ErrorMessage).Distinct().ToArray() }
        );

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;

        return context.Response.WriteAsync(jsonResult);
    }
}
