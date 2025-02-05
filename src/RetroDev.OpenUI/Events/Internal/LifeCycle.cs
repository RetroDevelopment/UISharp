using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.Events.Internal;

internal class LifeCycle
{
    /// <summary>
    /// The step of the UI main loop pipeline.
    /// </summary>
    internal enum State
    {
        /// <summary>
        /// Initializing the UI (rendering loop not started yet).
        /// </summary>
        INIT,

        /// <summary>
        /// Polling events
        /// </summary>
        EVENT_POLL,

        /// <summary>
        /// Measuring drawing areas before rendering.
        /// </summary>
        MEASURE,

        /// <summary>
        /// Rendering the UI.
        /// </summary>
        RENDERING,

        /// <summary>
        /// Quitting the application.
        /// </summary>
        QUIT
    };

    private int? _uiThreadId = null;
    private static readonly object s_lockObject = new();

    /// <summary>
    /// The current lifecycle state.
    /// </summary>
    public State CurrentState { get; set; }

    /// <summary>
    /// Marks the calling thread as a UI thread.
    /// </summary>
    public void RegisterUIThread()
    {
        lock (s_lockObject)
        {
            _uiThreadId ??= GetCurrentThreadId();
        }
    }

    /// <summary>
    /// Ensures that the caller of this method is running on the UI thread.
    /// The first time this method is called, the calling thread is registered as UI thread.
    /// </summary>
    /// <exception cref="InvalidUIThreadException">If the caller of this method is not on the UI thread.</exception>
    public void ThrowIfNotOnUIThread()
    {
        if (_uiThreadId == null) throw new InvalidOperationException("UI thread not registered");

        if (_uiThreadId != GetCurrentThreadId())
        {
            throw new InvalidUIThreadException();
        }
    }

    /// <summary>
    /// Ensures that the application is rendering.
    /// </summary>
    /// <exception cref="LifeCycleException">If not in rendering phase.</exception>
    public void ThrowIfNotOnRenderingPhase()
    {
        ThrowIfNotOnUIThread();

        if (CurrentState != State.RENDERING)
        {
            throw new LifeCycleException($"Cannot render during lifecycle state {CurrentState}");
        }
    }

    /// <summary>
    /// Ensures that a property is not set on the wrong lifecycle state.
    /// </summary>
    /// <exception cref="LifeCycleException">If not setting a property on the right lifecycle state.</exception>
    public void ThrowIfPropertyCannotBeSet()
    {
        ThrowIfNotOnUIThread();

        if (CurrentState != State.EVENT_POLL && CurrentState != State.INIT)
        {
            throw new LifeCycleException($"Cannot set properties during lifecycle state {CurrentState}");
        }
    }

    /// <summary>
    /// Whether we should quit the application.
    /// </summary>
    /// <returns>Whether to quit the application.</returns>
    public bool IsQuitting()
    {
        return CurrentState == State.QUIT;
    }

    private static int GetCurrentThreadId() => Environment.CurrentManagedThreadId;
}
