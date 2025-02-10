using RetroDev.OpenUI.Core.Windowing.Events;

namespace RetroDev.OpenUI.Core.Windowing;

/// <summary>
/// The event system used in a given application.
/// </summary>
public interface IEventSystem
{
    /// <summary>
    /// An event that indicates to quit the application.
    /// The only way to trigger this event is that there is a call to <see cref="Quit"/>, because
    /// quitting the application must be managed by the <see cref="Application"/> class and not implicity by
    /// the event system underneath, unless there are specific needs or interrupts.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, EventArgs> ApplicationQuit;

    /// <summary>
    /// Mouse button press inside the given window.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>> MousePress;

    /// <summary>
    /// Mouse button release inside the given window.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>> MouseRelease;

    /// <summary>
    /// Mouse position changed inside the given window.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>> MouseMove;

    /// <summary>
    /// Key is pressed inside the given window.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<KeyEventArgs>> KeyPress;

    /// <summary>
    /// Key is released inside the given window.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<KeyEventArgs>> KeyRelease;

    /// <summary>
    /// Text is inserted from keyboard. This event is useful for text insert in edit boxes or other text insert
    /// UI components. The text input event must take into account key combinations like SHIFT + LETTER, and map it
    /// to the appropriate string depending on keyboard layout.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<TextInputEventArgs>> TextInput;

    /// <summary>
    /// The window has been moved.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<WindowMoveEventArgs>> WindowMove;

    /// <summary>
    /// The window has been resized.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<WindowResizeEventArgs>> WindowResize;

    /// <summary>
    /// The window is shown.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>> WindowOpen;

    /// <summary>
    /// The window is closed by clicking on the close button or by shutting it down.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>> WindowClose;

    /// <summary>
    /// The window is maximized.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>> WindowMaximized;

    /// <summary>
    /// The window is minimized.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>> WindowMinimized;

    /// <summary>
    /// The mouse wheel has been moved.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseWheelEventArgs>> MouseWheel;

    /// <summary>
    /// Before rendering.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, EventArgs> BeforeRender;

    /// <summary>
    /// Rendering is needed.
    /// </summary>
    event TypeSafeEventHandler<IEventSystem, EventArgs> Render;

    /// <summary>
    /// Processes all the pending events.
    /// </summary>
    void ProcessEvents();

    /// <summary>
    /// Terminates the event queue processing.
    /// </summary>
    void Quit();

    // TODO: we will need a more sophysticated event queue for pushing custom events (dispatcher?)
    /// <summary>
    /// Push the invalidate rendering event.
    /// </summary>
    void InvalidateRendering();
}
