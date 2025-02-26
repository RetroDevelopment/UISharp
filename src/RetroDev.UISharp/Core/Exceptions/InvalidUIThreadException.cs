using FreeTypeSharp;

namespace RetroDev.UISharp.Core.Exceptions;

/// <summary>
/// Indicates that a UI operation has been performed on a thread that is not the UI thread.
/// </summary>
/// <param name="threadName">The name of the thread invoking this exception.</param>
public class InvalidUIThreadException(string threadName) :
    UIException($"UI operation must be performed on the UI thread, but currently on thread {threadName}");
