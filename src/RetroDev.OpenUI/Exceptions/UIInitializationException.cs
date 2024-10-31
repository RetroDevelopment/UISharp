namespace RetroDev.OpenUI.Exceptions;

/// <summary>
/// An exception that occurs when UI initialization fails.
/// </summary>
/// <param name="message">The error message.</param>
public class UIInitializationException(string message) : UIException(message);
