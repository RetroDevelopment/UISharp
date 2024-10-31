using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Events;

namespace RetroDev.OpenUI.Core;

/// <summary>
/// The event system used in a given application.
/// </summary>
public interface IEventSystem
{
    /// <summary>
    /// An event that indicates to quit the application.
    /// </summary>
    event EventHandler ApplicationQuit;

    /// <summary>
    /// Mouse button press inside the given window.
    /// </summary>
    event EventHandler<WindowEventArgs<MouseEventArgs>> MousePress;

    /// <summary>
    /// Mouse button release inside the given window.
    /// </summary>
    event EventHandler<WindowEventArgs<MouseEventArgs>> MouseRelease;

    /// <summary>
    /// Mouse position changed inside the given window.
    /// </summary>
    event EventHandler<WindowEventArgs<MouseEventArgs>> MouseMove;

    /// <summary>
    /// Key is pressed inside the given window.
    /// </summary>
    event EventHandler<WindowEventArgs<KeyEventArgs>> KeyPress;

    /// <summary>
    /// Key is released inside the given window.
    /// </summary>
    event EventHandler<WindowEventArgs<KeyEventArgs>> KeyRelease;

    /// <summary>
    /// Processes all the pending events.
    /// </summary>
    void ProcessEvents();
}
