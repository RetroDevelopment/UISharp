namespace RetroDev.OpenUI.Core.Logging;

/// <summary>
/// A logger implementation that suppresses all log messages.
/// Useful for scenarios where logging is not required.
/// </summary>
public class SilentLogger : ILogger
{
    /// <inheritdoc />
    public Verbosity Verbosity { get; set; }

    /// <inheritdoc />
    public void LogError(string message) { }

    /// <inheritdoc />
    public void LogWarning(string message) { }

    /// <inheritdoc />
    public void LogInfo(string message) { }

    /// <inheritdoc />
    public void LogDebug(string message) { }

    /// <inheritdoc />
    public void LogVerbose(string message) { }

}
