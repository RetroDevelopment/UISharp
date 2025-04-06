using System.Collections.Concurrent;
using RetroDev.UISharp.Core.Exceptions;

namespace RetroDev.UISharp.Core.Windowing;

using DispatchCallback = Action;

/// <summary>
/// Establishes what operation is done in what thread.
/// This class is thread-safe.
/// </summary>
public class ThreadDispatcher
{
    // TODO: Add a Finalize() method to properly run finalization of disposable resources on UIThread at the righht moment?.

    private readonly Thread _uiThread;
    private readonly ConcurrentQueue<DispatchCallback> _dispatcherQueue = [];
    private readonly LifeCycle _lifeCycle;

    /// <summary>
    /// Creates a new <see cref="ThreadDispatcher"/>.
    /// </summary>
    /// <param name="lifeCycle">The application lifecycle.</param>
    /// <exception cref="InvalidUIThreadException">If the dispatcher is created on a thread pool thread.</exception>
    public ThreadDispatcher(LifeCycle lifeCycle)
    {
        if (Thread.CurrentThread.IsThreadPoolThread)
        {
            throw new InvalidUIThreadException($"{nameof(ThreadDispatcher)} must not be created on a thread pool thread.");
        }

        _uiThread = Thread.CurrentThread;
        _lifeCycle = lifeCycle;

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
            throw new InvalidUIThreadException($"UI operations must be performed on the UI thread (named '{_uiThread.Name ?? "Unknown"}'), but currently on thread: '{Thread.CurrentThread.Name ?? "Unknown"}'.");
        }
    }

    /// <summary>
    /// Schedules the execution of the given <paramref name="callback"/> ensuring that it is executed in
    /// the UI thread in the <see cref="LifeCycle.State.EVENT_POLL"/> state.
    /// </summary>
    /// <param name="callback">The callback to execute.</param>
    public void Schedule(DispatchCallback callback)
    {
        if (Thread.CurrentThread != _uiThread || _lifeCycle.CurrentState != LifeCycle.State.EVENT_POLL)
        {
            _dispatcherQueue.Enqueue(callback);
        }
        else
        {
            callback();
        }
    }

    /// <summary>
    /// Ensures the given action runs on the UI thread, deferring if necessary.
    /// </summary>
    public void RunOnUIThread(DispatchCallback callback)
    {
        Schedule(callback);
    }

    internal void ProcessEventQueue()
    {
        ThrowIfNotOnUIThread();

        while (_dispatcherQueue.TryDequeue(out var callback))
        {
            callback();
        }
    }
}
