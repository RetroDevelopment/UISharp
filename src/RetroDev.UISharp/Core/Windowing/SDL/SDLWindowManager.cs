using RetroDev.UISharp.Core.Contexts;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Exceptions;
using RetroDev.UISharp.Core.Logging;
using static SDL2.SDL;

namespace RetroDev.UISharp.Core.Windowing.SDL;

/// <summary>
/// Window manager implemented using SDL2.
/// </summary>
public class SDLWindowManager : IWindowManager
{
    private static bool s_isInitialized = false;
    private static object s_lock = new();

    private readonly ThreadDispatcher _dispatcher;
    private readonly ILogger _logger;
    private readonly Dictionary<MouseCursor, nint> _cursorCache = [];

    private MouseCursor _mouseCursor = MouseCursor.Unknown;

    /// <summary>
    /// Gets the size of the main display.
    /// </summary>
    public Size ScreenSize
    {
        get
        {
            _dispatcher.ThrowIfNotOnUIThread();
            SDL_GetCurrentDisplayMode(0, out var displayMode);
            return new Size(displayMode.w, displayMode.h);
        }
    }

    /// <summary>
    /// The mouse cursor shape to display.
    /// </summary>
    public MouseCursor Cursor
    {
        set
        {
            if (_mouseCursor == value) return;
            _mouseCursor = value;
            if (_cursorCache.TryGetValue(value, out var sdlCursor))
            {
                SDL_SetCursor(sdlCursor);
            }
            else
            {
                var cursor = SDL_CreateSystemCursor(SDLMouseCursorMapping.ToKeyButton(value));
                if (cursor == IntPtr.Zero) throw new InvalidOperationException($"SDL failed to create cursor {value}");
                _cursorCache.Add(value, cursor);
                SDL_SetCursor(cursor);
            }
        }
        get
        {
            return _mouseCursor;
        }
    }

    /// <summary>
    /// The event system emitting event using SDL2.
    /// </summary>
    public IEventSystem EventSystem { get; }

    /// <summary>
    /// Creates a new window manager using SDL2.
    /// </summary>
    /// <param name="dispatcher">Manages the UI thread and dispatches UI operations from other thread to the UI thread.</param>
    /// <param name="logger">The logger used to log SDL events.</param>
    public SDLWindowManager(ThreadDispatcher dispatcher, ILogger logger)
    {
        _dispatcher = dispatcher;
        _logger = logger;
        EventSystem = new SDLEventSystem(dispatcher, logger);
    }

    /// <summary>
    /// Initializes the UI environment.
    /// </summary>
    /// <exception cref="UIInitializationException">If the initialization fails or the environment has already been initialized.</exception>
    public void Initialize()
    {
        lock (s_lock)
        {
            if (s_isInitialized)
            {
                throw new UIInitializationException("SDL environment can only be initialized once and in one thread.");
            }

            LoggingUtils.SDLCheck(() => SDL_InitSubSystem(SDL_INIT_VIDEO), _logger);
            SDL_GetVersion(out var version);
            _logger.LogInfo($"Window manger based on SDL {version.major}.{version.minor}.{version.patch}");
            Cursor = MouseCursor.Default;
            s_isInitialized = true;
        }
    }

    /// <summary>
    /// Creates a new window without showing it.
    /// </summary>
    /// <param name="context">
    /// The rendering context for the window. It must be compatible with <see langword="this" /> <see cref="IWindowManager"/>
    /// implementation.
    /// </param>
    /// <returns>The window identifier.</returns>
    /// <exception cref="UIInitializationException">If the given <paramref name="context"/> does not implement <see cref="ISDLRenderingContext"/>.</exception>
    public IWindowId CreateWindow(IRenderingContext context)
    {
        if (context is ISDLRenderingContext sdlContext)
        {
            return sdlContext.WindowId;
        }

        throw new UIInitializationException("Rendering context not compatible with SDL.");
    }

    /// <summary>
    /// Show the window.
    /// </summary>
    /// <param name="windowId">The ID of the window to show.</param>
    /// <exception cref="ArgumentException">If the <paramref name="windowId"/> is not a <see cref="SDLWindowId"/>.</exception>
    public void ShowWindow(IWindowId windowId) =>
        ExecuteOnWindow(windowId, id => SDL_ShowWindow(id.Handle));

    /// <summary>
    /// Hides the window.
    /// </summary>
    /// <param name="windowId">The ID of the window to hide.</param>
    /// <exception cref="ArgumentException">If the <paramref name="windowId"/> is not a <see cref="SDLWindowId"/>.</exception>
    public void HideWindow(IWindowId windowId) =>
        ExecuteOnWindow(windowId, id => SDL_HideWindow(id.Handle));

    /// <summary>
    /// Sets the rendering area of the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The ID of the window for which to set the title.</param>
    /// <param name="renderingArea">The window position and size.</param>
    /// <exception cref="ArgumentException">If the <paramref name="windowId"/> is not a <see cref="SDLWindowId"/>.</exception>
    public void SetRenderingArea(IWindowId windowId, Area renderingArea)
    {
        ExecuteOnWindow(windowId, id =>
        {
            var top = 0;
            var left = 0;
            var bottom = 0;
            var right = 0;
            LoggingUtils.SDLCheck(() => SDL_GetWindowBordersSize(id.Handle, out top, out left, out bottom, out right), _logger);
            var position = renderingArea.TopLeft;
            var size = renderingArea.Size;
            SDL_SetWindowPosition(id.Handle, (int)position.X + left, (int)position.Y + top);
            SDL_SetWindowSize(id.Handle, (int)size.Width, (int)size.Height);
        });
    }

    /// <summary>
    /// Sets the title of the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The ID of the window for which to set the title.</param>
    /// <param name="title">The window title.</param>
    /// <exception cref="ArgumentException">If the <paramref name="windowId"/> is not a <see cref="SDLWindowId"/>.</exception>
    public void SetTitle(IWindowId windowId, string title) =>
        ExecuteOnWindow(windowId, id => SDL_SetWindowTitle(id.Handle, title));

    /// <summary>
    /// Sets the opacity of the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set opacity.</param>
    /// <param name="opacity">The opacity, where 0 means fully transparent and 255 means fully opaque.</param>
    public void SetOpacity(IWindowId windowId, byte opacity) =>
        ExecuteOnWindow(windowId, id => SDL_SetWindowOpacity(id.Handle, opacity / 255.0f));

    /// <summary>
    /// Sets whether the window with the given <paramref name="windowId"/> is reiszable.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set if it is resizable.</param>
    /// <param name="resizable"><see langword="true" /> if the window is resizable, otherwise <see langword="false" />.</param>
    public void SetResizable(IWindowId windowId, bool resizable) =>
        ExecuteOnWindow(windowId, id => SDL_SetWindowResizable(id.Handle, resizable ? SDL_bool.SDL_TRUE : SDL_bool.SDL_FALSE));

    /// <summary>
    /// Focuses the window with <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to focus.</param>
    public void FocusWindow(IWindowId windowId) =>
        ExecuteOnWindow(windowId, id => SDL_RaiseWindow(id.Handle));

    /// <summary>
    /// Maximizes the window with <paramref name="windowID"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to maximize.</param>
    public void Maximize(IWindowId windowID) =>
        ExecuteOnWindow(windowID, id => SDL_MaximizeWindow(id.Handle));

    /// <summary>
    /// Minimizes the window with <paramref name="windowID"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to minimize.</param>
    public void Minimize(IWindowId windowID) =>
        ExecuteOnWindow(windowID, id => SDL_MinimizeWindow(id.Handle));

    /// <summary>
    /// Sets the window with <paramref name="windowID"/> in full screen mode.
    /// </summary>
    /// <param name="windowId">The identifier of the window to set in full screen.</param>
    public void SetFullScreen(IWindowId windowID) =>
        ExecuteOnWindow(windowID, id => SDL_SetWindowFullscreen(id.Handle, (uint)SDL_WindowFlags.SDL_WINDOW_FULLSCREEN));

    /// <summary>
    /// Restores the window with <paramref name="windowID"/> from full screen mode.
    /// </summary>
    /// <param name="windowId">The identifier of the window to restore from full screen.</param>
    public void RestoreFullScreen(IWindowId windowID) =>
        ExecuteOnWindow(windowID, id => SDL_SetWindowFullscreen(id.Handle, 0));

    /// <summary>
    /// Restores maximize and minimize state of the window with <paramref name="windowID"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to maximize.</param>
    public void RestoreWindow(IWindowId windowId) =>
        ExecuteOnWindow(windowId, id => SDL_RestoreWindow(id.Handle));

    /// <summary>
    /// Copies the given <paramref name="text"/> to the clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    public void CopyToClipboard(string text)
    {
        _dispatcher.ThrowIfNotOnUIThread();
        LoggingUtils.SDLCheck(() => SDL_SetClipboardText(text), _logger);
    }

    /// <summary>
    /// Gets the text in the clipboard.
    /// </summary>
    /// <returns>The clipboard text.</returns>
    public string GetClipboardContent()
    {
        _dispatcher.ThrowIfNotOnUIThread();
        return SDL_GetClipboardText();
    }

    /// <summary>
    /// Dispose the window and deallocates all the graphical resources.
    /// </summary>
    public void Shutdown()
    {
        _dispatcher.ThrowIfNotOnUIThread();
        // TODO: dispose all rendering engines
        // if (_window != nint.Zero) SDL2.SDL.SDL_DestroyWindow(_window);
        foreach (var cursor in _cursorCache)
        {
            SDL_FreeCursor(cursor.Value);
        }

        SDL_Quit();
    }

    private void ExecuteOnWindow(IWindowId windowId, Action<SDLWindowId> action)
    {
        _dispatcher.ThrowIfNotOnUIThread();
        if (windowId is SDLWindowId sdlWindowId)
        {
            action(sdlWindowId);
        }
        else
        {
            throw new ArgumentException($"The given ID {windowId} is not an SDL window ID");
        }
    }

    private TResult ExecuteOnWindow<TResult>(IWindowId windowId, Func<SDLWindowId, TResult> callback)
    {
        _dispatcher.ThrowIfNotOnUIThread();
        if (windowId is SDLWindowId sdlWindowId)
        {
            return callback(sdlWindowId);
        }
        else
        {
            throw new ArgumentException($"The given ID {windowId} is not an SDL window ID");
        }
    }
}
