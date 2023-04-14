using System;

namespace Fabillio.Common.Exceptions.Exceptions;

[Serializable]
public class MembershipNotFoundException : Exception
{
    public MembershipNotFoundException(Guid membershipId)
        : base($"Membership {membershipId} was not found") { }
}
