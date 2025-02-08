using RetroDev.OpenUI.Core.Contexts;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Logging;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.Utils;
using static SDL2.SDL;

namespace RetroDev.OpenUI.Core.Windowing.SDL;

/// <summary>
/// Window manager implemented using SDL2.
/// </summary>
public class SDLWindowManager : IWindowManager
{
    private static bool s_isInitialized = false;
    private static object s_lock = new();

    private readonly Action _ensureOnUIThread;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the size of the main display.
    /// </summary>
    public Size ScreenSize
    {
        get
        {
            _ensureOnUIThread();
            SDL_GetCurrentDisplayMode(0, out var displayMode);
            return new Size(displayMode.w, displayMode.h);
        }
    }

    /// <summary>
    /// The event system emitting event using SDL2.
    /// </summary>
    public IEventSystem EventSystem { get; }

    /// <summary>
    /// Creates a new window manager using SDL2.
    /// </summary>
    /// <param name="application">The owner application.</param>
    public SDLWindowManager(Application application)
    {
        _ensureOnUIThread = () => application.LifeCycle.ThrowIfNotOnUIThread();
        _logger = application.Logger;
        EventSystem = new SDLEventSystem(application);
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

            LoggingUtils.SDLCheck(() => SDL2.SDL.SDL_InitSubSystem(SDL_INIT_VIDEO), _logger);
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

        // _application.LifeCycle.ThrowIfNotOnUIThread();
        // 
        // var top = 0;
        // var left = 0;
        // var bottom = 0;
        // var right = 0;
        // LoggingUtils.SDLCheck(() => SDL.SDL_GetWindowBordersSize(_window, out top, out left, out bottom, out right), _application.Logger);
        // SDL.SDL_GetWindowPosition(_window, out var x, out var y);
        // SDL.SDL_GetWindowSize(_window, out var width, out var height);
        // return new Area(new Point(x - left, y - top), new Size(width, height));
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
    /// Dispose the window and deallocates all the graphical resources.
    /// </summary>
    public void Shutdown()
    {
        _ensureOnUIThread();
        // TODO: dispose all rendering engines
        // if (_window != nint.Zero) SDL2.SDL.SDL_DestroyWindow(_window);
        SDL_Quit();
    }

    private void ExecuteOnWindow(IWindowId windowId, Action<SDLWindowId> action)
    {
        _ensureOnUIThread();
        if (windowId is SDLWindowId sdlWindowId)
        {
            action(sdlWindowId);
        }
        else
        {
            throw new ArgumentException($"The given ID {windowId} is not an SDL window ID");
        }
    }
}
