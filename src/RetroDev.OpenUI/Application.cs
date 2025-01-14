using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Events.Internal;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Internal;
using RetroDev.OpenUI.Logging;
using RetroDev.OpenUI.Resources;
using RetroDev.OpenUI.Themes;
using RetroDev.OpenUI.UIDefinition;

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

    /// <summary>
    /// The logging injected implementation.
    /// </summary>
    public ILogger Logger { get; set; }

    /// <summary>
    /// A set of services to manage font.
    /// </summary>
    public IFontServices FontServices => new FontServices();

    /// <summary>
    /// Manages all resources.
    /// </summary>
    public IResourceManager ResourceManager { get; }

    /// <summary>
    /// The UIDefinition language manager, loading and generating xml UIDefinition files.
    /// </summary>
    public UIDefinitionManager UIDefinitionManager => new UIDefinitionManager(this);

    /// <summary>
    /// The main theme used in the application.
    /// </summary>
    public Theme Theme { get; set; }

    /// <summary>
    /// The primary screen size.
    /// </summary>
    public Size ScreenSize => new(800, 600); //TODO: real screen size

    internal LifeCycle LifeCycle { get; } = new();

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="uIEnvironment">The UI environment used to manage the main application status.</param>
    /// <param name="eventSystem">The event system used in this application.</param>
    /// <param name="resourceManager">The object that loads resources from the project.</param>
    /// <param name="logger">The logging implementation.</param>
    /// <remarks>The application, as well as all the UI related operations, must run in the same thread as this constructor is invoked.</remarks>
    public Application(IUIEnvironment? uIEnvironment = null,
                       IEventSystem? eventSystem = null,
                       IResourceManager? resourceManager = null,
                       ILogger? logger = null)
    {
        _uiEnvironment = uIEnvironment ?? new SDLUIEnvironment(this);
        _eventSystem = eventSystem ?? new SDLEventSystem(this);
        ResourceManager = resourceManager ?? new EmbeddedResourceManager();
        Theme = new Theme(new Dictionary<string, Color>()); // TODO: load from files
        Logger = logger ?? new ConsoleLogger();
        LifeCycle.RegisterUIThread();
        LifeCycle.CurrentState = LifeCycle.State.INIT;
        _uiEnvironment.Initialize();
    }

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

        Logger.LogInfo("Application started");
        _eventSystem.ApplicationQuit += (_, _) => _shoudQuit = true;
        _eventSystem.BeforeRender += EventSystem_BeforeRender;
        _eventSystem.InvalidateRendering();

        while (!_shoudQuit)
        {
            LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
            _eventSystem.ProcessEvents();
        }

        LifeCycle.CurrentState = LifeCycle.State.QUIT;
        Logger.LogInfo("Application terminated");
    }

    /// <summary>
    /// Creates and shows the window with type <typeparamref name="TWindow"/>.
    /// If <typeparamref name="TWindow"/> is FooWindow, creates an insteance of FooWindow and initializes it
    /// with the value provided in ResourceManager["foo"], which is expected to start with the fooWindow tag.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to show.</typeparam>
    /// <returns>The window instance</returns>
    /// <exception cref="InvalidOperationException">If it was not possible to locate the window.</exception>
    public TWindow ShowWindow<TWindow>() where TWindow : Window
    {
        var resourceName = typeof(TWindow).Name;

        if (resourceName.ToLower().EndsWith("window"))
        {
            resourceName = resourceName.Substring(0, resourceName.Length - "window".Length).ToLower();
        }

        return ShowWindow<TWindow>(resourceName);
    }

    /// <summary>
    /// Creates and shows the window with type <typeparamref name="TWindow"/>  and it initializes it from the
    /// window resource identified by <paramref name="windowName"/>.
    /// </summary>
    /// <typeparam name="TWindow">The type of the window to show.</typeparam>
    /// <param name="windowName">The window resouce identifier.</param>
    /// <returns>The window instance</returns>
    /// <exception cref="InvalidOperationException">If it was not possible to locate the window.</exception>
    public TWindow ShowWindow<TWindow>(string windowName) where TWindow : Window
    {
        Logger.LogInfo($"Showing window {windowName}");
        var windowXmlDefinition = ResourceManager.Windows[windowName];
        var component = UIDefinitionManager.CreateUIComponent(windowXmlDefinition);
        if (component is not TWindow) throw new InvalidOperationException($"Expected a window of type {typeof(TWindow)} but type {component.GetType()} found instead");
        var window = (TWindow)component;
        window.Visibility.Value = ComponentVisibility.Visible;
        return window;
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

    private void EventSystem_BeforeRender(IEventSystem sender, EventArgs e)
    {
        _windows.ForEach(w => w.Validate());
        _windows.ForEach(_w => _w.RepositionChildren());
        LifeCycle.CurrentState = LifeCycle.State.RENDERING;
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
