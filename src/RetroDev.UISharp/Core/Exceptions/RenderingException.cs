namespace RetroDev.UISharp.Core.Exceptions;

/// <summary>
/// An exception that occurs when UI rendering fails.
/// </summary>
/// <param name="message">The error message.</param>
public class RenderingException(string message) : UIException(message);
