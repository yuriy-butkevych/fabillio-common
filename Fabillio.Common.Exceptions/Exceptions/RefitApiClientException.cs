using System;

namespace Fabillio.Common.Exceptions.Exceptions;

[Serializable]
public class RefitApiClientException : Exception
{
    public RefitApiClientException(string message) : base(message)
        => message = message.Trim();

}
