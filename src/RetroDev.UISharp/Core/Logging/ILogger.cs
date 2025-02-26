namespace RetroDev.UISharp.Core.Logging;

/// <summary>
/// Defines a logging interface for capturing and handling log messages at various levels.
/// Implement this interface to provide custom logging mechanisms.
/// </summary>
public interface ILogger
{
    /// <summary>
    /// Specifies the logger verbosity.
    /// </summary>
    public Verbosity Verbosity { get; set; }

    /// <summary>
    /// Logs an error message.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    void LogError(string message);

    /// <summary>
    /// Logs a warning message.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    void LogWarning(string message);

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The informational message to log.</param>
    void LogInfo(string message);

    /// <summary>
    /// Logs a debug message, typically used for development and troubleshooting.
    /// </summary>
    /// <param name="message">The debug message to log.</param>
    void LogDebug(string message);

    /// <summary>
    /// Logs a verbose message, typically used for development and troubleshooting.
    /// </summary>
    /// <param name="message">The verbose message to log.</param>
    void LogVerbose(string message);
}
