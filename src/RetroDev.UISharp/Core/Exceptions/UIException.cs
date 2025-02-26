namespace RetroDev.UISharp.Core.Exceptions;

/// <summary>
/// The common class for exceptions thrown by the UISharp framework.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public abstract class UIException(string message, Exception? innerException = null) : Exception(message, innerException);
