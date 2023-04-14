using System;

namespace Fabillio.Common.Exceptions.Exceptions;

[Serializable]
public class FailedToStartSessionException : Exception
{
    public FailedToStartSessionException(string message = "Unable to initialize session.")
        : base(message) { }
}
