using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Core.Windowing.Events;
using static SDL2.SDL;

namespace RetroDev.UISharp.Core.Windowing.SDL;

/// <summary>
/// Manages UI events using SDL.
/// </summary>
internal class SDLEventSystem : IEventSystem
{
    private enum SDL_CustomEventType : uint
    {
        SDL_QUIT = SDL_EventType.SDL_USEREVENT + 1,
        SDL_SIGNAL = SDL_EventType.SDL_USEREVENT + 2
    }

    private readonly ThreadDispatcher _dispatcher;
    private readonly Stopwatch _stopwatch;
    private readonly ILogger _logger;
    private readonly SDL_EventFilter _eventFilter;
    private readonly object _signalLock = new();
    private bool _signaled = false;
    private bool _isRendering = false;

    /// <summary>
    /// An event that indicates to quit the application.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs>? ApplicationQuit;

    /// <summary>
    /// An event that indicates that rendering is needed.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, EventArgs>? RenderNeeded;

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
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowCloseRequest;

    /// <summary>
    /// The window is maximized.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowMaximized;

    /// <summary>
    /// The window is minimized.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowMinimized;

    /// <summary>
    /// The window maximize/minimzed status is removed.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowRestored;

    /// <summary>
    /// The window gains focus.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowFocusGain;

    /// <summary>
    /// The window looses focus.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<EventArgs>>? WindowFocusLost;

    /// <summary>
    /// The mouse wheel has been moved.
    /// </summary>
    public event TypeSafeEventHandler<IEventSystem, WindowEventArgs<MouseWheelEventArgs>>? MouseWheel;

    /// <summary>
    /// Creates a new event system based on SDL.
    /// </summary>
    /// <param name="dispatcher">Manages the UI thread and dispatches UI operations from other thread to the UI thread.</param>
    /// <param name="logger">The logger used to log events.</param>
    public SDLEventSystem(ThreadDispatcher dispatcher, ILogger logger)
    {
        _dispatcher = dispatcher;
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
        _logger = logger;
        _eventFilter = new SDL_EventFilter(EventCallback);
        SDL_StartTextInput();
        SDL_AddEventWatch(_eventFilter, IntPtr.Zero);
    }

    /// <summary>
    /// Processes all the pending events.
    /// This method must be blocking the calling thread waiting for events, then process all the events and exit.
    /// </summary>
    public void ProcessEvents()
    {
        _dispatcher.ThrowIfNotOnUIThread();
        SDL_WaitEvent(out var currentEvent);

        _stopwatch.Start();
        // TODO: add more events

        while (SDL_PollEvent(out currentEvent) != 0) ;
    }

    /// <summary>
    /// Awakes the main UI thread if it is blocked by <see cref="ProcessEvents"/>.
    /// This method is thread safe.
    /// </summary>
    public void Signal()
    {
        lock (_signalLock)
        {
            if (_signaled) return;
            _signaled = true;
            var signalEvent = new SDL_Event();
            signalEvent.type = (SDL_EventType)SDL_CustomEventType.SDL_SIGNAL;
            SDL_PushEvent(ref signalEvent);
        }
    }

    /// <summary>
    /// Terminates the event queue processing.
    /// </summary>
    public void Quit()
    {
        _dispatcher?.ThrowIfNotOnUIThread();
        var quitEvent = new SDL_Event();
        quitEvent.type = (SDL_EventType)SDL_CustomEventType.SDL_QUIT;
        SDL_PushEvent(ref quitEvent);
        ApplicationQuit?.Invoke(this, EventArgs.Empty);
        SDL_StopTextInput(); // TODO: move this into a dispose method
        SDL_DelEventWatch(_eventFilter, IntPtr.Zero);
    }

    private int EventCallback(nint userdata, nint sdlevent)
    {
        var currentEvent = Marshal.PtrToStructure<SDL_Event>(sdlevent);
        _dispatcher.Schedule(() => HandleEvent(currentEvent));
        return 1;
    }

    private void HandleEvent(SDL_Event currentEvent)
    {
        if ((int)currentEvent.type == (int)SDL_CustomEventType.SDL_QUIT) return;

        switch (currentEvent.type)
        {
            case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                var buttonDownEvent = currentEvent.button;
                var mouseButtonDownWindowId = GetWidnowIdFromButtonEvent(buttonDownEvent);
                var mouseButtonDownButton = GetMouseButton(buttonDownEvent);
                var mouseButtonDownArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonDownWindowId,
                                                                              new MouseEventArgs(new Point(buttonDownEvent.x, buttonDownEvent.y),
                                                                                                 new Point(buttonDownEvent.x, buttonDownEvent.y),
                                                                                                 mouseButtonDownButton,
                                                                                                 buttonDownEvent.clicks));
                mouseButtonDownArgs.Log("mouseDown", _logger);
                MousePress?.Invoke(this, mouseButtonDownArgs);
                break;

            case SDL_EventType.SDL_MOUSEBUTTONUP:
                var buttonUpEvent = currentEvent.button;
                var mouseButtonUpWindowId = GetWidnowIdFromButtonEvent(buttonUpEvent);
                var mouseButtonUpButton = GetMouseButton(buttonUpEvent);
                var mouseButtonUpArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonUpWindowId,
                                                                            new MouseEventArgs(new Point(buttonUpEvent.x, buttonUpEvent.y),
                                                                                               new Point(buttonUpEvent.x, buttonUpEvent.y),
                                                                                               mouseButtonUpButton,
                                                                                               buttonUpEvent.clicks));
                mouseButtonUpArgs.Log("mouseDown", _logger);
                MouseRelease?.Invoke(this, mouseButtonUpArgs);
                break;

            case SDL_EventType.SDL_MOUSEMOTION:
                var motionEvent = currentEvent.motion;
                var mouseButtonMoveWindowId = GetWidnowIdFromMotionEvent(motionEvent);
                var mouseButtonMoveButton = MouseButton.None;
                var mouseMotionMoveArgs = new WindowEventArgs<MouseEventArgs>(mouseButtonMoveWindowId,
                                                                              new MouseEventArgs(new Point(motionEvent.x, motionEvent.y),
                                                                                                 new Point(motionEvent.x, motionEvent.y),
                                                                                                 mouseButtonMoveButton,
                                                                                                 0));
                mouseMotionMoveArgs.Log("mouseMove", _logger);
                MouseMove?.Invoke(this, mouseMotionMoveArgs);
                break;

            case SDL_EventType.SDL_KEYDOWN:
                var keyDownEvent = currentEvent.key;
                var keyDownWindowId = GetWidnowIdFromKeyboardEvent(keyDownEvent);
                var keyDownKey = SDLKeyButtonMapping.ToKeyButton(keyDownEvent.keysym.sym);
                var keyDownArgs = new WindowEventArgs<KeyEventArgs>(keyDownWindowId, new KeyEventArgs(keyDownKey));
                keyDownArgs.Log("keyDown", _logger);
                KeyPress?.Invoke(this, keyDownArgs);
                break;

            case SDL_EventType.SDL_KEYUP:
                var keyUpEvent = currentEvent.key;
                var keyUpWindowId = GetWidnowIdFromKeyboardEvent(keyUpEvent);
                var keyUpKey = SDLKeyButtonMapping.ToKeyButton(keyUpEvent.keysym.sym);
                var keyUpArgs = new WindowEventArgs<KeyEventArgs>(keyUpWindowId, new KeyEventArgs(keyUpKey));
                keyUpArgs.Log("keyUp", _logger);
                KeyRelease?.Invoke(this, keyUpArgs);
                break;
            case SDL_EventType.SDL_TEXTINPUT:
                var textInputEvent = currentEvent.text;
                var inputText = GetString(textInputEvent);
                var inputTextWindowId = GetWindowIdFromTextInputEvent(textInputEvent);
                var textInputArgs = new WindowEventArgs<TextInputEventArgs>(inputTextWindowId, new TextInputEventArgs(inputText));
                TextInput?.Invoke(this, textInputArgs);
                textInputArgs.Log("textInput", _logger);
                break;
            case SDL_EventType.SDL_WINDOWEVENT:
                var windowEvent = currentEvent.window;
                var windowId = GetWindowIdFromWindowEvent(windowEvent);

                switch (windowEvent.windowEvent)
                {
                    case SDL_WindowEventID.SDL_WINDOWEVENT_MOVED:
                        var windowPtr = SDL_GetWindowFromID(windowEvent.windowID);
                        SDL_GetWindowBordersSize(windowPtr, out var top, out var left, out var bottom, out var right);
                        var movedArgs = new WindowEventArgs<WindowMoveEventArgs>(windowId, new WindowMoveEventArgs(new Point(windowEvent.data1 - left, windowEvent.data2 - top)));
                        movedArgs.Log("windowMove", _logger);
                        WindowMove?.Invoke(this, movedArgs);
                        break;

                    case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                    case SDL_WindowEventID.SDL_WINDOWEVENT_SIZE_CHANGED:
                        var resizedArgs = new WindowEventArgs<WindowResizeEventArgs>(windowId, new WindowResizeEventArgs(new Size(windowEvent.data1, windowEvent.data2)));
                        resizedArgs.Log("windowResize", _logger);
                        WindowResize?.Invoke(this, resizedArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE:
                        var closeArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        closeArgs.Log("windowClose", _logger);
                        WindowCloseRequest?.Invoke(this, closeArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_SHOWN:
                        var showArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        showArgs.Log("windowOpen", _logger);
                        WindowOpen?.Invoke(this, showArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_MAXIMIZED:
                        var maximizedArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        maximizedArgs.Log("windowMaximize", _logger);
                        WindowMaximized?.Invoke(this, maximizedArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_MINIMIZED:
                        var minimzeArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        minimzeArgs.Log("windowMinimize", _logger);
                        WindowMinimized?.Invoke(this, minimzeArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_RESTORED:
                        var restoreArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        restoreArgs.Log("windowRestore", _logger);
                        WindowRestored?.Invoke(this, restoreArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_GAINED:
                        var focusGainArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        focusGainArgs.Log("windowFocusGain", _logger);
                        WindowFocusGain?.Invoke(this, focusGainArgs);
                        break;
                    case SDL_WindowEventID.SDL_WINDOWEVENT_FOCUS_LOST:
                        var focusLostArgs = new WindowEventArgs<EventArgs>(windowId, EventArgs.Empty);
                        focusLostArgs.Log("windowFocusGain", _logger);
                        WindowFocusLost?.Invoke(this, focusLostArgs);
                        break;
                }
                break;
            case SDL_EventType.SDL_MOUSEWHEEL:
                var wheelEvent = currentEvent.wheel;
                var mouseWheelWindowId = GetWidnowIdFromWeelEvent(wheelEvent);
                SDL_GetMouseState(out var mouseX, out var mouseY);
                var mousePosition = new Point(mouseX, mouseY);
                var mouseWheelArgs = new WindowEventArgs<MouseWheelEventArgs>(mouseWheelWindowId, new MouseWheelEventArgs(wheelEvent.x, wheelEvent.y, mousePosition, mousePosition));
                mouseWheelArgs.Log("mouseWheel", _logger);
                MouseWheel?.Invoke(this, mouseWheelArgs);
                break;
        }

        if (!_isRendering)
        {
            _isRendering = true; // Avoid loops if an event is triggered inside the RenderNeeded handler.
            RenderNeeded?.Invoke(this, EventArgs.Empty);
            _isRendering = false;
            lock (_signalLock)
            {
                _signaled = false;
            }
            // Make sure the queue is empty (it the queue is filled e.g. during rendering)
            // The queue must be empty before exiting this event handling method because there is no guarantee on when next event handling call will be.
            _dispatcher.ProcessEventQueue();
        }
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
}
