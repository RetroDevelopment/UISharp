﻿using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Core.Helpers;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Exceptions;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Core.Windowing;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Core.Windowing.SDL;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Resources;
using RetroDev.UISharp.Presentation.Themes;
using RetroDev.UISharp.UIDefinition;
using RetroDev.UISharp.Windows;

namespace RetroDev.UISharp;

/// <summary>
/// The main class that starts, manages and run the application.
/// </summary>
public class Application : IDisposable
{
    private readonly List<Window> _windows = [];
    private bool _disposed = false;
    private bool _shoudQuit = false;

    /// <summary>
    /// Triggered when the application is started and ready to show windows and process events.
    /// </summary>
    public event TypeSafeEventHandler<Application, EventArgs>? ApplicationStarted;

    /// <summary>
    /// Triggered when the application is terminating.
    /// </summary>
    public event TypeSafeEventHandler<Application, EventArgs>? ApplicationQuit;

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
    /// The theme manager used in <see langword="this" /> <see cref="Application"/> to load and manage themes.
    /// </summary>
    public ThemeManager ThemeManager { get; }

    /// <summary>
    /// The primary screen size.
    /// </summary>
    public Size ScreenSize => WindowManager.ScreenSize;

    /// <summary>
    /// The default font for this application.
    /// </summary>
    // TODO: Consider using styles for default properties
    public UIProperty<Font> DefaultFont { get; }

    /// <summary>
    /// The list of all windows managed by <see langword="this" /> <see cref="Application"/>.
    /// </summary>
    public IEnumerable<Window> Windows => _windows;

    /// <summary>
    /// Manages the application life cycle.
    /// </summary>
    public LifeCycle LifeCycle { get; } = new();

    /// <summary>
    /// Manages the UI thread and dispatches UI operations from other thread to the UI thread.
    /// </summary>
    public ThreadDispatcher Dispatcher { get; }

    /// <summary>
    /// Whether <see langword="this" /> <see cref="Application"/> is ready to run and receive events.
    /// </summary>
    public bool Started { get; private set; } = false;

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="windowManager">The low-level implementation of window manager interacting with the OS to create and manage windows.</param>
    /// <param name="resourceManager">The object that loads resources from the project.</param>
    /// <param name="logger">The logging implementation.</param>
    /// <remarks>The application, as well as all the UI related operations, must run in the same thread as this constructor is invoked.</remarks>
    public Application(IWindowManager? windowManager = null,
                       IResourceManager? resourceManager = null,
                       ILogger? logger = null)
    {
        Logger = logger ?? new ConsoleLogger();
        Dispatcher = new ThreadDispatcher(LifeCycle);
        WindowManager = windowManager ?? new SDLWindowManager(Dispatcher, Logger);
        ResourceManager = resourceManager ?? new EmbeddedResourceManager();
        ThemeManager = new ThemeManager(this, ResourceManager.Themes);
        LifeCycle.CurrentState = LifeCycle.State.INIT;
        var font = new Font(this, "LiberationSans", 16, FontType.Regular);
        DefaultFont = new UIProperty<Font>(this, font, canReceiveBindingUpdates: false);

        ThemeManager.LoadTheme("uisharp-dark");
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
        Dispatcher.ThrowIfNotOnUIThread();

        Logger.LogInfo("Application started");
        EventSystem.ApplicationQuit += (_, _) => _shoudQuit = true;
        EventSystem.RenderNeeded += (_, _) => Render();
        Dispatcher.CallbackScheduled += (_, _) => EventSystem.Signal();
        Started = true;
        ApplicationStarted?.Invoke(this, EventArgs.Empty);
        EventSystem.Signal();

        while (!_shoudQuit)
        {
            RunUIEventPollLoop();
        }

        LifeCycle.CurrentState = LifeCycle.State.QUIT;
        ApplicationQuit?.Invoke(this, EventArgs.Empty);
        // TODO: run disposing here!
        Logger.LogInfo("Application terminated");
    }

    /// <summary>
    /// Creates and shows the window with type <typeparamref name="TWindow"/>.
    /// If <typeparamref name="TWindow"/> is FooWindow, creates an instance of FooWindow and initializes it
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
        window.Visibility.Value = UIObject.ComponentVisibility.Visible;
        return window;
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void Quit()
    {
        Dispatcher.ThrowIfNotOnUIThread();
        Logger.LogInfo("Application quit requested");
        EventSystem.Quit();
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
        Dispatcher.ThrowIfNotOnUIThread();
        LifeCycle.ThrowIfPropertyCannotBeSet();
        _windows.Add(window);
    }

    internal void RunUIEventPollLoop()
    {
        LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
        EventSystem.ProcessEvents();
    }

    private void Render()
    {
        var visibleWindows = _windows.Where(w => w.Visibility.Value == UIObject.ComponentVisibility.Visible).ToList();
        var shouldMeasure = true;
        while (shouldMeasure)
        {
            Dispatcher.ProcessEventQueue();
            LifeCycle.CurrentState = LifeCycle.State.MEASURE;
            visibleWindows.ForEach(w => w.Measure());
            visibleWindows.ForEach(w => w.Prepare(allPasses: false));
            LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
            var queueNotEmpty = Dispatcher.ProcessEventQueue();
            var invalidatedWindows = visibleWindows.Any(window => window.Invalidator.HasInvalidatedNodes);
            shouldMeasure = queueNotEmpty || invalidatedWindows;
        }

        LifeCycle.CurrentState = LifeCycle.State.RENDERING;
        visibleWindows.ForEach(w => w.EnsureZIndicesUpdated());
        visibleWindows.ForEach(w => w.Render());
        LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
        visibleWindows.ForEach(w => w.OnRenderDone());
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
