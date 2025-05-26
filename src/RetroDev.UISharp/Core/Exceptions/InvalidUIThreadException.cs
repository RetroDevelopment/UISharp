using FreeTypeSharp;

namespace RetroDev.UISharp.Core.Exceptions;

/// <summary>
/// Indicates that a UI operation has been performed on a thread that is not the UI thread.
/// </summary>
/// <param name="message">The error message.</param>
public class InvalidUIThreadException(string message) : UIException(message);
