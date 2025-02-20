namespace RetroDev.OpenUI.UI.Themes.Exceptions;

/// <summary>
/// An exception that occurs when parting of a theme file fails.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public class ThemeParseException(string message, Exception? innerException = null) : Exception(message, innerException);
