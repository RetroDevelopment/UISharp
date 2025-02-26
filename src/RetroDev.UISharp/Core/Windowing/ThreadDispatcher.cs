using RetroDev.UISharp.Core.Exceptions;

namespace RetroDev.UISharp.Core.Windowing;

/// <summary>
/// Establishes what operation is done in what thread.
/// </summary>
public class ThreadDispatcher
{
    // TODO: Add RunOnUIThread() to defer execution of UI operation from non UI thread into UI thread
    // TODO: Add a DispatchUIThread() to consume the derefferd events
    // TODO: Add a Finalize() method to properly run finalization of disposable resources on UIThread at the righht moment?.

    private readonly Thread _uiThread;

    /// <summary>
    /// Creates a new <see cref="ThreadDispatcher"/>.
    /// </summary>
    /// <exception cref="InvalidUIThreadException">If the dispatcher is created on a thread pool thread.</exception>
    public ThreadDispatcher()
    {
        if (Thread.CurrentThread.IsThreadPoolThread)
        {
            throw new InvalidUIThreadException($"{nameof(ThreadDispatcher)} must not be created on a thread pool thread.");
        }

        _uiThread = Thread.CurrentThread;

        // Assign a meaningful name for debugging if it hasn't been set
        if (string.IsNullOrEmpty(_uiThread.Name))
        {
            _uiThread.Name = "UIThread";
        }
    }

    /// <summary>
    /// Ensures that the caller of this method is running on the UI thread.
    /// The UI thread is the thread in which this instance has been created.
    /// </summary>
    /// <exception cref="InvalidUIThreadException">If the caller is not on the UI thread.</exception>
    public void ThrowIfNotOnUIThread()
    {
        if (_uiThread != Thread.CurrentThread)
        {
            throw new InvalidUIThreadException($"Expected thread: {_uiThread.Name ?? "Unknown"}, but was: {Thread.CurrentThread.Name ?? "Unknown"}.");
        }
    }
}
