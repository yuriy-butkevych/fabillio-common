using System;

namespace Fabillio.Common.Exceptions.Exceptions;

[Serializable]
public class InvalidSamvirkOperationException : Exception
{
    public InvalidSamvirkOperationException(string operation) : base(operation) { }
}
