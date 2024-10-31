using System.Runtime.CompilerServices;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Events;
using SDL2;
using static SDL2.SDL;

namespace RetroDev.OpenUI.Core.Internal;

/// <summary>
/// Manages UI events using SDL.
/// </summary>
internal class SDLEventSystem : IEventSystem
{
    /// <summary>
    /// An event that indicates to quit the application.
    /// </summary>
    public event EventHandler ApplicationQuit = (_, _) => { };

    /// <summary>
    /// Mouse button press on a window.
    /// </summary>
    public event EventHandler<WindowEventArgs<MouseEventArgs>> MousePress = (_, _) => { };

    /// <summary>
    /// Mouse button release on a window.
    /// </summary>
    public event EventHandler<WindowEventArgs<MouseEventArgs>> MouseRelease = (_, _) => { };

    /// <summary>
    /// Mouse position changed inside the given window.
    /// </summary>
    public event EventHandler<WindowEventArgs<MouseEventArgs>> MouseMove = (_, _) => { };

    /// <summary>
    /// Key is pressed inside the given window.
    /// </summary>
    public event EventHandler<WindowEventArgs<KeyEventArgs>> KeyPress = (_, _) => { };

    /// <summary>
    /// Key is released inside the given window.
    /// </summary>
    public event EventHandler<WindowEventArgs<KeyEventArgs>> KeyRelease = (_, _) => { };

    /// <summary>
    /// Process all the events in the event queue.
    /// </summary>
    public void ProcessEvents()
    {
        // TODO: add more events
        while (SDL.SDL_PollEvent(out var currentEvent) != 0)
        {
            switch (currentEvent.type)
            {
                case SDL.SDL_EventType.SDL_QUIT:
                    ApplicationQuit.Invoke(this, EventArgs.Empty);
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    var buttonDownEvent = currentEvent.button;
                    var mouseButtonDownWindowId = GetWidnowIdFromButtonEvent(buttonDownEvent);
                    var mouseButtonDownButton = GetMouseButton(buttonDownEvent);
                    MousePress.Invoke(this, new(mouseButtonDownWindowId, new(new(buttonDownEvent.x, buttonDownEvent.y), new(buttonDownEvent.x, buttonDownEvent.y), mouseButtonDownButton)));
                    break;

                case SDL.SDL_EventType.SDL_MOUSEBUTTONUP:
                    var buttonUpEvent = currentEvent.button;
                    var mouseButtonUpWindowId = GetWidnowIdFromButtonEvent(buttonUpEvent);
                    var mouseButtonUpButton = GetMouseButton(buttonUpEvent);
                    MousePress.Invoke(this, new(mouseButtonUpWindowId, new(new(buttonUpEvent.x, buttonUpEvent.y), new(buttonUpEvent.x, buttonUpEvent.y), mouseButtonUpButton)));
                    break;

                case SDL.SDL_EventType.SDL_MOUSEMOTION:
                    var motionEvent = currentEvent.motion;
                    var mouseButtonMoveWindowId = GetWidnowIdFromMotionEvent(motionEvent);
                    var mouseButtonMoveButton = MouseButton.None;
                    MousePress.Invoke(this, new(mouseButtonMoveWindowId, new(new(motionEvent.x, motionEvent.y), new(motionEvent.x, motionEvent.y), mouseButtonMoveButton)));
                    break;

                case SDL.SDL_EventType.SDL_KEYDOWN:
                    var keyDownEvent = currentEvent.key;
                    var keyDownWindowId = GetWidnowIdFromKeyboardEvent(keyDownEvent);
                    var keyDownKey = KeyMapping.ToKeyButton(keyDownEvent.keysym.sym);
                    KeyPress.Invoke(this, new(keyDownWindowId, new(keyDownKey)));
                    break;

                case SDL.SDL_EventType.SDL_KEYUP:
                    var keyUpEvent = currentEvent.key;
                    var keyUpWindowId = GetWidnowIdFromKeyboardEvent(keyUpEvent);
                    var keyUpKey = KeyMapping.ToKeyButton(keyUpEvent.keysym.sym);
                    KeyPress.Invoke(this, new(keyUpWindowId, new(keyUpKey)));
                    break;
            }
        }
    }

    private static SDLWindowId GetWidnowIdFromButtonEvent(SDL.SDL_MouseButtonEvent mouseButtonEvent) =>
        new((int)mouseButtonEvent.windowID);

    private static SDLWindowId GetWidnowIdFromMotionEvent(SDL.SDL_MouseMotionEvent mouseMotionEvent) =>
        new((int)mouseMotionEvent.windowID);

    private static SDLWindowId GetWidnowIdFromKeyboardEvent(SDL.SDL_KeyboardEvent keyboardEvent) =>
        new((int)keyboardEvent.windowID);

    private static MouseButton GetMouseButton(SDL.SDL_MouseButtonEvent currentEvent) => currentEvent.button switch
    {
        (byte)SDL.SDL_BUTTON_LEFT => MouseButton.Left,
        (byte)SDL.SDL_BUTTON_MIDDLE => MouseButton.Middle,
        (byte)SDL.SDL_BUTTON_RIGHT => MouseButton.Right,
        _ => MouseButton.Unknown,
        //TODO: add more buttons
    };
}
