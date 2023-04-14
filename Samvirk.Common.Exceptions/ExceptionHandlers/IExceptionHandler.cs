using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Fabillio.Common.Exceptions.ExceptionHandlers;

public interface IExceptionHandler<in TException> where TException : Exception
{
    Task HandleException(TException exception, HttpContext context);
}
