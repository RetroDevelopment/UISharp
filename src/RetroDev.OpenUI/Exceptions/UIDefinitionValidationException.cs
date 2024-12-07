namespace RetroDev.OpenUI.Exceptions;

/// <summary>
/// An exception that occurs when the UI definition validation fails.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public class UIDefinitionValidationException(string message, Exception? innerException = null) : UIException(message, innerException);
