using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Internal;
using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI;

/// <summary>
/// The main class that starts, manages and run the application.
/// </summary>
public class Application : IDisposable
{
    private bool _disposed = false;
    private readonly List<Window> _windows = [];
    private bool _shoudQuit = false;

    // ===========================
    // Dependency injection fields
    // ===========================
    private readonly IUIEnvironment _uiEnvironment;
    internal readonly IEventSystem _eventSystem;
    internal readonly ISVGRenderingEngine _svgEngine;
    internal readonly IFontRenderingEngine _fontEngine;

    internal LifeCycle LifeCycle { get; } = new();

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="uIEnvironment">The UI environment used to manage the main application status.</param>
    /// <param name="eventSystem">The event system used in this application.</param>
    /// <param name="svgEngine">The SVG rendering engine responsible of rendering svg images into a byte array.</param>
    /// <param name="fontEngine">The rendering engine responsible of rendering text into a byte array.</param>
    /// <remarks>The application, as well as all the UI related operations, must run in the same thread as this constructor is invoked.</remarks>
    public Application(IUIEnvironment uIEnvironment, IEventSystem eventSystem, ISVGRenderingEngine svgEngine, IFontRenderingEngine fontEngine)
    {
        _uiEnvironment = uIEnvironment;
        _eventSystem = eventSystem;
        _svgEngine = svgEngine;
        _fontEngine = fontEngine;
        LifeCycle.RegisterUIThread();
        LifeCycle.CurrentState = LifeCycle.State.INIT;
        _uiEnvironment.Initialize();
    }

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <remarks>The application, as well as all the UI related operations, must run in the same thread as this constructor is invoked.</remarks>
    public Application() : this(new SDLUIEnvironment(), new SDLEventSystem(), new SvgRenderingEngine(), new SkiaFontRenderingEngine()) { }

    /// <summary>
    /// Start the application loop. The calling thread will be the UI 
    /// </summary>
    /// <exception cref="InvalidUIThreadException">
    /// If the caller of this method is not on the UI thread. This happens if another thread invokes the <see cref="Run"/> or
    /// <see cref="ThrowIfNotOnUIThread"/> before the call to Run(), for example, if a Window is created on another thread.
    /// </exception>
    /// <exception cref="UIInitializationException">If the UI initialization fails.</exception>
    public void Run()
    {
        LifeCycle.ThrowIfNotOnUIThread();
        _eventSystem.ApplicationQuit += (_, _) => _shoudQuit = true;

        while (!_shoudQuit)
        {
            LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
            _eventSystem.ProcessEvents();
            LifeCycle.CurrentState = LifeCycle.State.RENDERING;
            _windows.ForEach(window => { window.Render(); });
        }

        LifeCycle.CurrentState = LifeCycle.State.QUIT;
    }

    /// <summary>
    /// Disposes all the application resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes all the application resources.
    /// </summary>
    /// <param name="disposing">Whether the caller is <see cref="Dispose()"/></param>
    protected void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            DisposeManagedResources();
        }

        DisposeUnmanagedResources();
        _disposed = true;
    }

    /// <summary>
    /// Adds a window to the application.
    /// </summary>
    /// <param name="window"></param>
    internal void AddWindow(Window window)
    {
        LifeCycle.ThrowIfNotOnUIThread();
        LifeCycle.ThrowIfPropertyCannotBeSet();
        _windows.Add(window);
    }

    private void DisposeManagedResources() { }

    private void DisposeUnmanagedResources()
    {
        _windows.ForEach((window) => window.Shutdown());
        _uiEnvironment.Shutdown();
    }

    ~Application()
    {
        Dispose(false);
    }
}
