namespace RetroDev.OpenUI.Core.Logging;

/// <summary>
/// A logger implementation that writes log messages to the console.
/// Includes support for color-coded log levels and a timestamped format.
/// </summary>
public class ConsoleLogger : ILogger
{
    /// <inheritdoc />
    public Verbosity Verbosity { get; set; }

    /// <inheritdoc />
    public void LogError(string message)
    {
        WriteLog(Verbosity.Error, "ERROR", message, ConsoleColor.Red);
    }

    /// <inheritdoc />
    public void LogWarning(string message)
    {
        WriteLog(Verbosity.Warning, "WARNING", message, ConsoleColor.Yellow);
    }

    /// <inheritdoc />
    public void LogInfo(string message)
    {
        WriteLog(Verbosity.Info, "INFO", message, ConsoleColor.Blue);
    }

    /// <inheritdoc />
    public void LogDebug(string message)
    {
        WriteLog(Verbosity.Debug, "DEBUG", message, ConsoleColor.White);
    }

    /// <inheritdoc />
    public void LogVerbose(string message)
    {
        WriteLog(Verbosity.Verbose, "VERBOSE", message, ConsoleColor.Gray);
    }

    /// <summary>
    /// Writes a formatted log message to the console.
    /// </summary>
    /// <param name="verbosity">The logging verbisity.</param>
    /// <param name="level">The log level (e.g., INFO, WARNING, ERROR, DEBUG).</param>
    /// <param name="message">The message to log.</param>
    /// <param name="color">The console color to use for the message.</param>
    private void WriteLog(Verbosity verbosity, string level, string message, ConsoleColor color)
    {
        if (!ShouldLog(verbosity)) return;

        var timestamp = DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss:fff");
        var formattedMessage = $"({timestamp}) [{level}] {message}";

        var previousColor = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(formattedMessage);
        Console.ForegroundColor = previousColor;
    }

    private bool ShouldLog(Verbosity verbosity)
    {
        var maximumVerbosity = Verbosity == Verbosity.Default ? Verbosity.Info : Verbosity;
        return verbosity <= maximumVerbosity;
    }
}
