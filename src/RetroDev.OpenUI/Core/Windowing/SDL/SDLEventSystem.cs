using System.Diagnostics;
using System.Text;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.Core.Windowing.Events.Internal;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.Utils;
using static SDL2.SDL;

namespace RetroDev.OpenUI.Core.Windowing.SDL;

/// <summary>
/// Manages UI events using SDL.
/// </summary>
/// <param name="application">The sapplication using this event system.</param>
internal class SDLEventSystem(Application application) : IEventSystem
{
    private enum SDL_CustomEventType : uint
    {
        SDL_INVALIDATE_RENDERING = SDL_EventType.SDL_USEREVENT + 1, // Custom event type, starting from SDL_USEREVENT
        SDL_QUIT = SDL_EventType.SDL_USEREVENT + 2,
    }

    private Application _application = application;
    private bool _invalidated = false; // TODO: add invalidation logic per component. And maybe glScissor for retained mode. Or detect actual UI property change. If not do not invalidate.
    private bool _quit = false;

    /// <summary>
    /// An event that indicates to quit the application.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs>? ApplicationQuit;

    /// <summary>
    /// Mouse button press on a window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>>? MousePress;

    /// <summary>
    /// Mouse button release on a window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>>? MouseRelease;

    /// <summary>
    /// Mouse position changed inside the given window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseEventArgs>>? MouseMove;

    /// <summary>
    /// Key is pressed inside the given window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<KeyEventArgs>>? KeyPress;

    /// <summary>
    /// Key is released inside the given window.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<KeyEventArgs>>? KeyRelease;

    /// <summary>
    /// Text is inserted from keyboard. This event is useful for text insert in edit boxes or other text insert
    /// UI components. The text input event must take into account key combinations like SHIFT + LETTER, and map it
    /// to the appropriate string depending on keyboard layout.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<TextInputEventArgs>>? TextInput;

    /// <summary>
    /// The window has been moved.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<WindowMoveEventArgs>>? WindowMove;

    /// <summary>
    /// The window has been resized.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<WindowResizeEventArgs>>? WindowResize;

    /// <summary>
    /// The window is shown.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowOpen;

    /// <summary>
    /// The window is closed by clicking on the close button or by shutting it down.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowClose;

    /// <summary>
    /// The window is maximized.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowMaximized;

    /// <summary>
    /// The window is minimized.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowMinimized;

    /// <summary>
    /// The mouse wheel has been moved.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseWheelEventArgs>>? MouseWheel;

    /// <summary>
    /// Before rendering.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs>? BeforeRender;

    /// <summary>
    /// Rendering is needed.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs>? Render;

    /// <summary>
    /// Process all the events in the event queue.
    /// </summary>
    public void ProcessEvents()
    {
        SDL_StartTextInput();
        SDL_WaitEvent(out var currentEvent);

        var stopwatch = Stopwatch.StartNew();
        // TODO: add more events

        do
        {
            switch (currentEvent.type)
            {
                case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                    var buttonDownEvent = currentEvent.button;
                    var mouseButtonDownWindowId = GetWidnowIdFromButtonEvent(buttonDownEvent);
                    var mouseButtonDownButton = GetMouseButton(buttonDownEvent);
                    var mouseButtonDownArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonDownWindowId, new(new(buttonDownEvent.x, buttonDownEvent.y), new(buttonDownEvent.x, buttonDownEvent.y), mouseButtonDownButton));
                    mouseButtonDownArgs.Log("mouseDown", _application.Logger);
                    MousePress?.Invoke(this, mouseButtonDownArgs);
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
                    MouseRelease?.Invoke(this, mouseButtonUpArgs);
                    break;

                case SDL_EventType.SDL_MOUSEMOTION:
                    var motionEvent = currentEvent.motion;
                    var mouseButtonMoveWindowId = GetWidnowIdFromMotionEvent(motionEvent);
                    var mouseButtonMoveButton = MouseButton.None;
                    var mouseMotionMoveArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonMoveWindowId,
                                                                                  new MouseEventArgs(new Point(motionEvent.x, motionEvent.y),
                                                                                                     new Point(motionEvent.x, motionEvent.y), mouseButtonMoveButton));
                    mouseMotionMoveArgs.Log("mouseMove", _application.Logger);
                    MouseMove?.Invoke(this, mouseMotionMoveArgs);
                    break;

                case SDL_EventType.SDL_KEYDOWN:
                    var keyDownEvent = currentEvent.key;
                    var keyDownWindowId = GetWidnowIdFromKeyboardEvent(keyDownEvent);
                    var keyDownKey = KeyMapping.ToKeyButton(keyDownEvent.keysym.sym);
                    var keyDownArgs = new WindowEventArgs<KeyEventArgs>(keyDownWindowId, new KeyEventArgs(keyDownKey));
                    keyDownArgs.Log("keyDown", _application.Logger);
                    KeyPress?.Invoke(this, keyDownArgs);
                    break;

                case SDL_EventType.SDL_KEYUP:
                    var keyUpEvent = currentEvent.key;
                    var keyUpWindowId = GetWidnowIdFromKeyboardEvent(keyUpEvent);
                    var keyUpKey = KeyMapping.ToKeyButton(keyUpEvent.keysym.sym);
                    var keyUpArgs = new WindowEventArgs<KeyEventArgs>(keyUpWindowId, new KeyEventArgs(keyUpKey));
                    keyUpArgs.Log("keyUp", _application.Logger);
                    KeyRelease?.Invoke(this, keyUpArgs);
                    break;
                case SDL_EventType.SDL_TEXTINPUT:
                    var textInputEvent = currentEvent.text;
                    var inputText = GetString(textInputEvent);
                    var inputTextWindowId = GetWindowIdFromTextInputEvent(textInputEvent);
                    var textInputArgs = new WindowEventArgs<TextInputEventArgs>(inputTextWindowId, new TextInputEventArgs(inputText));
                    TextInput?.Invoke(this, textInputArgs);
                    textInputArgs.Log("textInput", _application.Logger);
                    break;
                case SDL_EventType.SDL_WINDOWEVENT:
                    // TODO: fix issue that UI thread freezes while resizing window. Either use a thread to constantly
                    // poll the window size or use platform specific solutions.
                    var windowEvent = currentEvent.window;
                    var windowId = GetWindowIdFromWindowEvent(windowEvent);

                    switch (windowEvent.windowEvent)
                    {
                        case SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                            var windowPtr = SDL_GetWindowFromID(windowEvent.windowID);
                            SDL_GetWindowBordersSize(windowPtr, out var top, out var left, out var bottom, out var right);
                            var movedArgs = new WindowEventArgs<WindowMoveEventArgs>(windowId, new WindowMoveEventArgs(new Point(windowEvent.data1 - left, windowEvent.data2 - top)));
                            movedArgs.Log("windowMove", _application.Logger);
                            WindowMove?.Invoke(this, movedArgs);
                            break;

                        case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                        case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                            var resizedArgs = new WindowEventArgs<WindowResizeEventArgs>(windowId, new WindowResizeEventArgs(new Size(windowEvent.data1, windowEvent.data2)));
                            resizedArgs.Log("windowResize", _application.Logger);
                            WindowResize?.Invoke(this, resizedArgs);
                            break;
                        case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                        case SDL_WindowEventID.SDL_WINDOWEVENT_HIDDEN:
                            var closeArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                            closeArgs.Log("windowClose", _application.Logger);
                            WindowClose?.Invoke(this, closeArgs);
                            break;
                        case SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                            var showArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                            showArgs.Log("windowOpen", _application.Logger);
                            WindowOpen?.Invoke(this, showArgs);
                            break;
                        case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                            var maximizedArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                            maximizedArgs.Log("windowMaximize", _application.Logger);
                            WindowMaximized?.Invoke(this, maximizedArgs);
                            break;
                        case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                            var minimzeArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                            minimzeArgs.Log("windowMinimize", _application.Logger);
                            WindowMinimized?.Invoke(this, minimzeArgs);
                            break;
                    }
                    break;
                case SDL_EventType.SDL_MOUSEWHEEL:
                    var wheelEvent = currentEvent.wheel;
                    var mouseWheelWindowId = GetWidnowIdFromWeelEvent(wheelEvent);
                    var mouseWheelArgs = new WindowEventArgs<MouseWheelEventArgs>(mouseWheelWindowId, new MouseWheelEventArgs(wheelEvent.x, wheelEvent.y));
                    mouseWheelArgs.Log("mouseWheel", _application.Logger);
                    MouseWheel?.Invoke(this, mouseWheelArgs);
                    break;
            }
            // Exit the loop if more than 10 milliseconds have passed to render frame (otherwise the loop can last too long skipping frames)
            if (stopwatch.ElapsedMilliseconds > 10) break;
        } while (SDL_PollEvent(out currentEvent) != 0 && !_quit);

        SDL_StopTextInput();
        if (_invalidated) EmitRenderingEvents();
    }

    /// <summary>
    /// Push the invalidate rendering event.
    /// </summary>
    public void InvalidateRendering()
    {
        if (_invalidated) return;
        var invalidateEvent = new SDL_Event();
        invalidateEvent.type = (SDL_EventType)SDL_CustomEventType.SDL_INVALIDATE_RENDERING; // Set the event type
        SDL_PushEvent(ref invalidateEvent);
        _invalidated = true;
    }

    /// <summary>
    /// Quits the event processing.
    /// </summary>
    public void Quit()
    {
        _quit = true;
        ApplicationQuit?.Invoke(this, EventArgs.Empty);
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
        new(mouseButtonEvent.windowID);

    private static SDLWindowId GetWidnowIdFromMotionEvent(SDL_MouseMotionEvent mouseMotionEvent) =>
        new(mouseMotionEvent.windowID);

    private static SDLWindowId GetWidnowIdFromKeyboardEvent(SDL_KeyboardEvent keyboardEvent) =>
        new(keyboardEvent.windowID);

    private static SDLWindowId GetWindowIdFromTextInputEvent(SDL_TextInputEvent textInputEvent) =>
        new(textInputEvent.windowID);

    private static SDLWindowId GetWindowIdFromWindowEvent(SDL_WindowEvent windowEvent) =>
        new(windowEvent.windowID);

    private static SDLWindowId GetWidnowIdFromWeelEvent(SDL_MouseWheelEvent mouseWheelEvent) =>
        new(mouseWheelEvent.windowID);

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
        BeforeRender?.Invoke(this, EventArgs.Empty);
        Render?.Invoke(this, EventArgs.Empty);
        _invalidated = false; // TODO: invalidated maybe won't be needed? In general we will have a more uniform custom event dispatcher
    }
}
