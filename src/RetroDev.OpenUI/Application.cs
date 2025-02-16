using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Core.Windowing;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.Core.Windowing.Events.Internal;
using RetroDev.OpenUI.Core.Windowing.SDL;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Logging;
using RetroDev.OpenUI.UI;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;
using RetroDev.OpenUI.UI.Resources;
using RetroDev.OpenUI.UI.Themes;
using RetroDev.OpenUI.UIDefinition;

namespace RetroDev.OpenUI;

/// <summary>
/// The main class that starts, manages and run the application.
/// </summary>
public class Application : IDisposable
{
    private readonly List<Window> _windows = [];
    private readonly ThemeParser _themeParser;
    private bool _disposed = false;
    private bool _shoudQuit = false;

    /// <summary>
    /// Triggered when the application is started and ready to show windows and process events.
    /// </summary>
    public event TypeSafeEventHandler<Application, EventArgs>? ApplicationStarted;

    /// <summary>
    /// The window manager interacting with the OS to create and manage systems.
    /// </summary>
    public IWindowManager WindowManager { get; }

    /// <summary>
    /// Event emitter.
    /// </summary>
    public IEventSystem EventSystem => WindowManager.EventSystem;

    /// <summary>
    /// The logging injected implementation.
    /// </summary>
    public ILogger Logger { get; set; }

    /// <summary>
    /// Manages all resources.
    /// </summary>
    public IResourceManager ResourceManager { get; }

    /// <summary>
    /// The UIDefinition language manager, loading and generating xml UIDefinition files.
    /// </summary>
    public UIDefinitionManager UIDefinitionManager => new(this);

    /// <summary>
    /// The main theme used in the application.
    /// </summary>
    public Theme Theme { get; }

    /// <summary>
    /// The primary screen size.
    /// </summary>
    public Size ScreenSize => WindowManager.ScreenSize;

    /// <summary>
    /// The default font for this application.
    /// </summary>
    // TODO: Consider using styles for default properties
    public BindableProperty<Font> DefaultFont { get; }

    /// <summary>
    /// The list of all windows managed by <see langword="this" /> <see cref="Application"/>.
    /// </summary>
    public IEnumerable<Window> Windows => _windows;

    internal LifeCycle LifeCycle { get; } = new();

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="windowManager">The low-level implementation of window manager interacting with the OS to create and manage windows.</param>
    /// <param name="resourceManager">The object that loads resources from the project.</param>
    /// <param name="logger">The logging implementation.</param>
    /// <param name="createTheme">
    /// The function that creates a <see cref="Theme"/>. The theme will be automatically created, so pass this function if you want to inject <see cref="Theme"/>
    /// with an instance of a class derived from <see cref="UI.Themes.Theme"/>.
    /// </param>
    /// <remarks>The application, as well as all the UI related operations, must run in the same thread as this constructor is invoked.</remarks>
    public Application(IWindowManager? windowManager = null,
                       IResourceManager? resourceManager = null,
                       ILogger? logger = null,
                       Func<Application, Theme>? createTheme = null)
    {
        LifeCycle.RegisterUIThread();
        Logger = logger ?? new ConsoleLogger();
        WindowManager = windowManager ?? new SDLWindowManager(this);
        ResourceManager = resourceManager ?? new EmbeddedResourceManager();
        Theme = createTheme != null ? createTheme(this) : new Theme(this);
        _themeParser = new ThemeParser(Theme);
        LifeCycle.CurrentState = LifeCycle.State.INIT;
        var font = new Font(this, "OpenSans", 18, FontType.Regular);
        DefaultFont = new BindableProperty<Font>(font, this, BindingType.SourceToDestination);

        LoadThemeResource("openui-dark");
        WindowManager.Initialize();
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
        EventSystem.ApplicationQuit += (_, _) => _shoudQuit = true;
        ApplicationStarted?.Invoke(this, EventArgs.Empty);
        EventSystem.BeforeRender += EventSystem_BeforeRender;
        EventSystem.InvalidateRendering();

        while (!_shoudQuit)
        {
            LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
            EventSystem.ProcessEvents();
        }

        LifeCycle.CurrentState = LifeCycle.State.QUIT;
        // TODO: run disposing here!
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
        window.Visibility.Value = UIComponent.ComponentVisibility.Visible;
        return window;
    }

    /// <summary>
    /// Loads a new theme from a theme resource.
    /// </summary>
    /// <param name="themeName">The theme resource name.</param>
    public void LoadThemeResource(string themeName)
    {
        _themeParser.Parse(ResourceManager.Themes[themeName]);
        Logger.LogInfo($"Theme: {themeName}");
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void Quit()
    {
        LifeCycle.ThrowIfNotOnUIThread();
        Logger.LogInfo("Application quit requested");
        EventSystem.Quit(emitQuitEvent: true);
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
        LifeCycle.CurrentState = LifeCycle.State.MEASURE;
        _windows.ForEach(w => w.Measure());
        LifeCycle.CurrentState = LifeCycle.State.RENDERING;
    }

    private void DisposeManagedResources() { }

    private void DisposeUnmanagedResources()
    {
        _windows.ForEach((window) => window.Shutdown());
        WindowManager.Shutdown();
    }

    ~Application()
    {
        Dispose(false);
    }
}
