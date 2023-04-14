using System;

namespace Fabillio.Common.Exceptions.Exceptions;

[Serializable]
public class PotentialFraudException : Exception
{
    public PotentialFraudException(string message = "User is blocked.") : base(message) { }
}
