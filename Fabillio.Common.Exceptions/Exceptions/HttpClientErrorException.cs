using System;

namespace Fabillio.Common.Exceptions.Exceptions;

[Serializable]
public class HttpClientErrorException : Exception
{
    public HttpClientErrorException(string message) : base(message)
    { }
}
