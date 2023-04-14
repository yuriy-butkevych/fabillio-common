namespace Fabillio.Common.Services.Enums;

public enum BackgroundCommandExecutionType
{
    /// <summary>
    /// auto select
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// all tasks after each signal
    /// </summary>
    Concurrently = 1,

    /// <summary>
    /// one by one
    /// </summary>
    Sequentially = 2
}
