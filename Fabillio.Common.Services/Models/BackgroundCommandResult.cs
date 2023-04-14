using System;
using Fabillio.Common.Services.Enums;

namespace Fabillio.Common.Services.Models;

public record BackgroundCommandResult(
    bool? Available = null,
    BackgroundCommandStatus? Status = null,
    DateTime? Started = null,
    string FullName = null,
    double? MinutesLeft = null,
    Guid? TaskId = null
);
