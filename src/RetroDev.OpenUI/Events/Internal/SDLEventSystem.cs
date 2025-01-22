using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Utils;
using SDL2;
using static SDL2.SDL;

namespace RetroDev.OpenUI.Events.Internal;

/// <summary>
/// Manages UI events using SDL.
/// </summary>
/// <param name="application">The sapplication using this event system.</param>
internal class SDLEventSystem(Application application) : IEventSystem
{
    private enum SDL_CustomEventType : uint
    {
        SDL_INVALIDE_RENDERING = SDL_EventType.SDL_USEREVENT + 1 // Custom event type, starting from SDL_USEREVENT
    }

    private Application _application = application;
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
    /// Text is inserted from keyboard. This event is useful for text insert in edit boxes or other text insert
    /// UI components. The text input event must take into account key combinations like SHIFT + LETTER, and map it
    /// to the appropriate string depending on keyboard layout.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<TextInputEventArgs>> TextInput = (_, _) => { };

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
        SDL.SDL_StartTextInput();
        SDL_WaitEvent(out var currentEvent);

        var stopwatch = Stopwatch.StartNew();
        // TODO: add more events
        do
        {
            switch (currentEvent.type)
            {
                case SDL_EventType.SDL_QUIT:
                    var quitEventArgs = EventArgs.Empty;
                    quitEventArgs.Log("quit", _application.Logger);
                    ApplicationQuit.Invoke(this, quitEventArgs);
                    break;

                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    var buttonDownEvent = currentEvent.button;
                    var mouseButtonDownWindowId = GetWidnowIdFromButtonEvent(buttonDownEvent);
                    var mouseButtonDownButton = GetMouseButton(buttonDownEvent);
                    var mouseButtonDownArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonDownWindowId, new(new(buttonDownEvent.x, buttonDownEvent.y), new(buttonDownEvent.x, buttonDownEvent.y), mouseButtonDownButton));
                    mouseButtonDownArgs.Log("mouseDown", _application.Logger);
                    MousePress.Invoke(this, mouseButtonDownArgs);
                    break;

                case SDL_EventType.SDL_MOUSEBUTTONUP:
                    var buttonUpEvent = currentEvent.button;
                    var mouseButtonUpWindowId = GetWidnowIdFromButtonEvent(buttonUpEvent);
                    var mouseButtonUpButton = GetMouseButton(buttonUpEvent);
                    var mouseButtonUpArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonUpWindowId,
                                                                                new MouseEventArgs(new Point(buttonUpEvent.x, buttonUpEvent.y),
                                                                                                   new Point(buttonUpEvent.x, buttonUpEvent.y),
                                                                                mouseButtonUpButton));
                    mouseButtonUpArgs.Log("mouseDown", _application.Logger);
                    MouseRelease.Invoke(this, mouseButtonUpArgs);
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    var motionEvent = currentEvent.motion;
                    var mouseButtonMoveWindowId = GetWidnowIdFromMotionEvent(motionEvent);
                    var mouseButtonMoveButton = MouseButton.None;
                    var mouseMotionMoveArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonMoveWindowId,
                                                                                  new MouseEventArgs(new Point(motionEvent.x, motionEvent.y),
                                                                                                     new Point(motionEvent.x, motionEvent.y), mouseButtonMoveButton));
                    mouseMotionMoveArgs.Log("mouseMove", _application.Logger);
                    MouseMove.Invoke(this, mouseMotionMoveArgs);
                    break;

                case SDL_EventType.SDL_KEYDOWN:
                    var keyDownEvent = currentEvent.key;
                    var keyDownWindowId = GetWidnowIdFromKeyboardEvent(keyDownEvent);
                    var keyDownKey = KeyMapping.ToKeyButton(keyDownEvent.keysym.sym);
                    var keyDownArgs = new WindowEventArgs<KeyEventArgs>(keyDownWindowId, new KeyEventArgs(keyDownKey));
                    keyDownArgs.Log("keyDown", _application.Logger);
                    KeyPress.Invoke(this, keyDownArgs);
                    break;

                case SDL_EventType.SDL_KEYUP:
                    var keyUpEvent = currentEvent.key;
                    var keyUpWindowId = GetWidnowIdFromKeyboardEvent(keyUpEvent);
                    var keyUpKey = KeyMapping.ToKeyButton(keyUpEvent.keysym.sym);
                    var keyUpArgs = new WindowEventArgs<KeyEventArgs>(keyUpWindowId, new KeyEventArgs(keyUpKey));
                    keyUpArgs.Log("keyUp", _application.Logger);
                    KeyRelease.Invoke(this, keyUpArgs);
                    break;
                case SDL_EventType.SDL_TEXTINPUT:
                    var textInputEvent = currentEvent.text;
                    var inputText = GetString(textInputEvent);
                    var inputTextWindowId = GetWindowIdFromTextInputEvent(textInputEvent);
                    var textInputArgs = new WindowEventArgs<TextInputEventArgs>(inputTextWindowId, new TextInputEventArgs(inputText));
                    TextInput.Invoke(this, textInputArgs);
                    textInputArgs.Log("textInput", _application.Logger);
                    break;
            }
            // Exit the loop if more than 10 milliseconds have passed to render frame (otherwise the loop can last too long skipping frames)
            if (stopwatch.ElapsedMilliseconds > 10) break;
        } while (SDL_PollEvent(out currentEvent) != 0);

        SDL.SDL_StopTextInput();
        if (_invalidated) EmitRenderingEvents();
    }

    /// <summary>
    /// Push the invalidate rendering event.
    /// </summary>
    public void InvalidateRendering()
    {
        if (_invalidated) return;
        var invalidateEvent = new SDL_Event();
        invalidateEvent.type = (SDL_EventType)SDL_CustomEventType.SDL_INVALIDE_RENDERING; // Set the event type
        SDL_PushEvent(ref invalidateEvent);
        _invalidated = true;
    }

    private static string GetString(SDL_TextInputEvent textInputEvent)
    {
        var inputText = string.Empty;

        unsafe
        {
            var textPtr = textInputEvent.text;

            // Determine the length of the null-terminated string
            var length = 0;
            while (textPtr[length] != 0)
            {
                length++;
            }

            // Allocate a managed byte array to hold the text
            var textBytes = new byte[length];

            // Copy the bytes from the unmanaged pointer to the managed array
            for (var i = 0; i < length; i++)
            {
                textBytes[i] = textPtr[i];
            }

            // Convert the byte array to a C# string
            inputText = Encoding.UTF8.GetString(textBytes);
        }

        return inputText;
    }

    private static SDLWindowId GetWidnowIdFromButtonEvent(SDL_MouseButtonEvent mouseButtonEvent) =>
        new((int)mouseButtonEvent.windowID);

    private static SDLWindowId GetWidnowIdFromMotionEvent(SDL_MouseMotionEvent mouseMotionEvent) =>
        new((int)mouseMotionEvent.windowID);

    private static SDLWindowId GetWidnowIdFromKeyboardEvent(SDL_KeyboardEvent keyboardEvent) =>
        new((int)keyboardEvent.windowID);

    private static SDLWindowId GetWindowIdFromTextInputEvent(SDL_TextInputEvent textInputEvent) =>
        new((int)textInputEvent.windowID);

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
