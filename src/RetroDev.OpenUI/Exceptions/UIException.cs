namespace RetroDev.OpenUI.Exceptions;

/// <summary>
/// The common class for exceptions thrown by the OpenUI framework.
/// </summary>
/// <param name="message">The error message.</param>
public abstract class UIException(string message) : Exception(message);
