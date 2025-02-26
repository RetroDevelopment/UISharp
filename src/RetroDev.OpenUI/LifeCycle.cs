using RetroDev.OpenUI.Core.Exceptions;

namespace RetroDev.OpenUI;

/// <summary>
/// Describes the application life cycle.
/// </summary>
public class LifeCycle
{
    /// <summary>
    /// The step of the UI main loop pipeline.
    /// </summary>
    public enum State
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

    /// <summary>
    /// The current lifecycle state.
    /// </summary>
    public State CurrentState { get; internal set; }

    /// <summary>
    /// Ensures that the application is rendering.
    /// </summary>
    /// <exception cref="LifeCycleException">If not in rendering phase.</exception>
    public void ThrowIfNotOnRenderingPhase()
    {
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
}
