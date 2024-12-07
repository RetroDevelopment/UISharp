using System.Diagnostics;
using System.Runtime.CompilerServices;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;
using RetroDev.OpenUI.Events;
using SDL2;
using static SDL2.SDL;

namespace RetroDev.OpenUI.Events.Internal;

/// <summary>
/// Manages UI events using SDL.
/// </summary>
internal class SDLEventSystem : IEventSystem
{
    private enum SDL_CustomEventType : uint
    {
        SDL_INVALIDE_RENDERING = SDL_EventType.SDL_USEREVENT + 1 // Custom event type, starting from SDL_USEREVENT
    }

    private bool _invalidated = true; // TODO: add invalidation logic per component. And maybe glScissor for retained mode. Or detect actual UI property change. If not do not invalidate.

    /// <summary>
    /// An event that indicates to quit the application.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs> ApplicationQuit = (_, _) => { };

    /// <summary>
    /// Mouse button press on a window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>> MousePress = (_, _) => { };

    /// <summary>
    /// Mouse button release on a window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>> MouseRelease = (_, _) => { };

    /// <summary>
    /// Mouse position changed inside the given window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>> MouseMove = (_, _) => { };

    /// <summary>
    /// Key is pressed inside the given window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<KeyEventArgs>> KeyPress = (_, _) => { };

    /// <summary>
    /// Key is released inside the given window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<KeyEventArgs>> KeyRelease = (_, _) => { };

    /// <summary>
    /// Before rendering.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs> BeforeRender = (_, _) => { };

    /// <summary>
    /// Rendering is needed.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs> Render = (_, _) => { };

    /// <summary>
    /// Process all the events in the event queue.
    /// </summary>
    public void ProcessEvents()
    {
        SDL_WaitEvent(out var currentEvent);

        var stopwatch = Stopwatch.StartNew();
        // TODO: add more events
        do
        {
            switch (currentEvent.type)
            {
                case SDL_EventType.SDL_QUIT:
                    ApplicationQuit.Invoke(this, EventArgs.Empty);
                    break;

                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    var buttonDownEvent = currentEvent.button;
                    var mouseButtonDownWindowId = GetWidnowIdFromButtonEvent(buttonDownEvent);
                    var mouseButtonDownButton = GetMouseButton(buttonDownEvent);
                    MousePress.Invoke(this, new(mouseButtonDownWindowId, new(new(buttonDownEvent.x, buttonDownEvent.y), new(buttonDownEvent.x, buttonDownEvent.y), mouseButtonDownButton)));
                    _invalidated = true;
                    break;

                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    var buttonUpEvent = currentEvent.button;
                    var mouseButtonUpWindowId = GetWidnowIdFromButtonEvent(buttonUpEvent);
                    var mouseButtonUpButton = GetMouseButton(buttonUpEvent);
                    MouseRelease.Invoke(this, new(mouseButtonUpWindowId, new(new(buttonUpEvent.x, buttonUpEvent.y), new(buttonUpEvent.x, buttonUpEvent.y), mouseButtonUpButton)));
                    _invalidated = true;
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    var motionEvent = currentEvent.motion;
                    var mouseButtonMoveWindowId = GetWidnowIdFromMotionEvent(motionEvent);
                    var mouseButtonMoveButton = MouseButton.None;
                    MouseMove.Invoke(this, new(mouseButtonMoveWindowId, new(new(motionEvent.x, motionEvent.y), new(motionEvent.x, motionEvent.y), mouseButtonMoveButton)));
                    _invalidated = true;
                    break;

                case SDL_EventType.SDL_KEYDOWN:
                    var keyDownEvent = currentEvent.key;
                    var keyDownWindowId = GetWidnowIdFromKeyboardEvent(keyDownEvent);
                    var keyDownKey = KeyMapping.ToKeyButton(keyDownEvent.keysym.sym);
                    KeyPress.Invoke(this, new(keyDownWindowId, new(keyDownKey)));
                    _invalidated = true;
                    break;

                case SDL_EventType.SDL_KEYUP:
                    var keyUpEvent = currentEvent.key;
                    var keyUpWindowId = GetWidnowIdFromKeyboardEvent(keyUpEvent);
                    var keyUpKey = KeyMapping.ToKeyButton(keyUpEvent.keysym.sym);
                    KeyRelease.Invoke(this, new(keyUpWindowId, new(keyUpKey)));
                    _invalidated = true;
                    break;
            }
            // Exit the loop if more than 10 milliseconds have passed to render frame (otherwise the loop can last too long skipping frames)
            if (stopwatch.ElapsedMilliseconds > 10) break;
        } while (SDL_PollEvent(out currentEvent) != 0);

        if (_invalidated) EmitRenderingEvents();
    }

    /// <summary>
    /// Push the invalidate rendering event.
    /// </summary>
    public void InvalidateRendering()
    {
        if (_invalidated) return;
        SDL_Event invalidateEvent = new SDL_Event();
        invalidateEvent.type = (SDL_EventType)SDL_CustomEventType.SDL_INVALIDE_RENDERING; // Set the event type
        SDL_PushEvent(ref invalidateEvent);
        _invalidated = true;
    }

    private static SDLWindowId GetWidnowIdFromButtonEvent(SDL_MouseButtonEvent mouseButtonEvent) =>
        new((int)mouseButtonEvent.windowID);

    private static SDLWindowId GetWidnowIdFromMotionEvent(SDL_MouseMotionEvent mouseMotionEvent) =>
        new((int)mouseMotionEvent.windowID);

    private static SDLWindowId GetWidnowIdFromKeyboardEvent(SDL_KeyboardEvent keyboardEvent) =>
        new((int)keyboardEvent.windowID);

    private static MouseButton GetMouseButton(SDL_MouseButtonEvent currentEvent) => currentEvent.button switch
    {
        (byte)SDL_BUTTON_LEFT => MouseButton.Left,
        (byte)SDL_BUTTON_MIDDLE => MouseButton.Middle,
        (byte)SDL_BUTTON_RIGHT => MouseButton.Right,
        _ => MouseButton.Unknown,
        //TODO: add more buttons
    };

    private void EmitRenderingEvents()
    {
        BeforeRender.Invoke(this, EventArgs.Empty);
        Render.Invoke(this, EventArgs.Empty);
        _invalidated = false; // TODO: invalidated maybe won't be needed? In general we will have a more uniform custom event dispatcher
    }
}
