namespace RetroDev.OpenUI.Core.Logging;

/// <summary>
/// Specifies the verbosity levels for logging.
/// Controls the amount and detail of log messages produced.
/// </summary>
public enum Verbosity
{
    /// <summary>
    /// The default verbosity level, which depdends on the implementation.
    /// </summary>
    Default,

    /// <summary>
    /// The most critical level of logging.
    /// Used to log errors and critical issues that may cause the application to fail or behave incorrectly.
    /// </summary>
    Error,

    /// <summary>
    /// Logs warnings about potential issues or non-critical problems.
    /// Useful for identifying areas of concern without immediate impact.
    /// </summary>
    Warning,

    /// <summary>
    /// Logs informational messages about the normal operation of the application.
    /// Useful for understanding the application's flow and key milestones.
    /// </summary>
    Info,

    /// <summary>
    /// Logs detailed messages intended for debugging purposes.
    /// Contains technical details that help troubleshoot issues during development or analysis.
    /// </summary>
    Debug,

    /// <summary>
    /// The highest level of verbosity.
    /// Logs all messages, including the most granular and detailed information about the application's operations.
    /// </summary>
    Verbose
}
