using RetroDev.OpenUI.Components.Core;
using RetroDev.OpenUI.Core.Contexts;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Windowing;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;
using RetroDev.OpenUI.Core.Windowing.SDL;
using RetroDev.OpenUI.UI;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Base;

namespace RetroDev.OpenUI.Components;

/// <summary>
/// Displays and manage a window.
/// </summary>
// TODO: add disposable or shutdown method to call renderingEngine.Shutdown();
[EditorSettings(allow: false)]
public class Window : UIRoot
{
    /// <summary>
    /// The default behavior when closing the window.
    /// </summary>
    public enum WindowCloseBehavior
    {
        /// <summary>
        /// Do nothing.
        /// </summary>
        None,

        /// <summary>
        /// Hides the window.
        /// </summary>
        HideWindow,

        /// <summary>
        /// Quits the whole application.
        /// </summary>
        QuitApplication
    };

    private readonly IRenderingEngine _renderingEngine;
    private readonly IWindowId _windowId;

    // If visibility changes, this flag is true so that the the window is actually displayed, but only
    // after, when the rendering area will be updated.
    // TODO: consider pushing the event in a priority queue to be done before rendering.
    // Let's see when we will have implement an event queue.
    private bool _scheduleVisibilityChange = false;

    /// <summary>
    /// Raised when <see langword="this" /> <see cref="Window"/> has been initialized.
    /// This happens when all the initial <see cref="UIComponent"/> have been added to the window.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs>? Initialized;

    /// <summary>
    /// Raised when the window is moved.
    /// </summary>
    public event TypeSafeEventHandler<Window, WindowMoveEventArgs>? WindowMove;

    /// <summary>
    /// Raised when the window size is changed.
    /// </summary>
    public event TypeSafeEventHandler<Window, WindowResizeEventArgs>? WindowResize;

    /// <summary>
    /// Raised when the window is shown.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs>? WindowOpen;

    /// <summary>
    /// Raised when the window is closed.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs>? WindowClose;

    /// <summary>
    /// Raised when the window is maximized.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs>? WindowMaximized;

    /// <summary>
    /// Raised when the window is minimized.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs>? WindowMinimized;

    /// <inheritdoc/>
    public override IEnumerable<UIWidget> Children => GetChildrenNodes();

    /// <summary>
    /// The window title.
    /// </summary>
    public UIProperty<Window, string> Title { get; }

    /// <summary>
    /// Whether the window is resizable.
    /// </summary>
    public UIProperty<Window, bool> Resizable { get; }

    /// <summary>
    /// The default action to execute when closing the window.
    /// By default, the first window created has the <see cref="WindowCloseBehavior.QuitApplication"/> and the subsequent
    /// windows have the <see cref="WindowCloseBehavior.HideWindow"/>.
    /// </summary>
    public UIProperty<Window, WindowCloseBehavior> CloseBehavior { get; }

    /// <summary>
    /// Creates a new window.
    /// </summary>
    /// <param name="application">The application owning this window.</param>
    /// <param name="renderingEngine">
    /// The rendering engine to render this window. By default the <see cref="OpenGLRenderingEngine"/> with <see cref="SDLOpenGLRenderingContext"/> is created.
    /// If you want to create another rendering engine (e.g. Vulkan) or you are not using the standard <see cref="SDLWindowManager"/>, you pass an instance of
    /// <see cref="IRenderingEngine"/>. If using opengl but not based on SDL, you can create a <see cref="OpenGLRenderingEngine"/> but you must implement and use your custom
    /// instance of <see cref="IRenderingContext"/>.
    /// </param>
    public Window(Application application, IRenderingEngine? renderingEngine = null) :
        base(application, visibility: ComponentVisibility.Collapsed, isFocusable: true, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        application.AddWindow(this);
        _renderingEngine = renderingEngine ?? new OpenGLRenderingEngine(application, new SDLOpenGLRenderingContext(application));
        _windowId = Application.WindowManager.CreateWindow(_renderingEngine.RenderingContext);

        Title = new UIProperty<Window, string>(this, string.Empty);
        Resizable = new UIProperty<Window, bool>(this, true);
        CloseBehavior = new UIProperty<Window, WindowCloseBehavior>(this, application.Windows.Count() > 1 ? WindowCloseBehavior.HideWindow : WindowCloseBehavior.QuitApplication);
        UpdateCloseBehavior();

        Application.EventSystem.Render += EventSystem_Render;
        Invalidate();

        BackgroundColor.BindDestinationToSource(Application.Theme.MainBackground);
        CloseBehavior.ValueChange += (_, _) => UpdateCloseBehavior();

        Visibility.ValueChange += Visibility_ValueChange;
        RenderingAreaChange += Window_RenderingAreaChange;

        application.EventSystem.MousePress += EventSystem_MousePress;
        application.EventSystem.MouseRelease += EventSystem_MouseRelease;
        application.EventSystem.MouseMove += EventSystem_MouseMove;
        application.EventSystem.KeyPress += EventSystem_KeyPress;
        application.EventSystem.KeyRelease += EventSystem_KeyRelease;
        application.EventSystem.TextInput += EventSystem_TextInput;
        application.EventSystem.WindowMove += EventSystem_WindowMove;
        application.EventSystem.WindowResize += EventSystem_WindowResize;
        application.EventSystem.MouseWheel += EventSystem_MouseWheel;
        application.EventSystem.WindowOpen += EventSystem_WindowOpen;
        application.EventSystem.WindowClose += EventSystem_WindowClose;
        application.EventSystem.WindowMaximized += EventSystem_WindowMaximized;
        application.EventSystem.WindowMinimized += EventSystem_WindowMinimized;
        // TODO remove events on dispose?
    }

    /// <summary>
    /// Removes a component from this window.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <returns><see langword="true" /> if the component has been successfully removed, otherwise <see langword="false"/>
    /// (for example, if the component has never been added).
    /// </returns>
    public bool RemoveComponent(UIWidget component)
    {
        return RemoveChildNode(component);
    }

    /// <summary>
    /// Notifies the <see cref="Initialized"/> listeners that <see langword="this" /> window has been initialized.
    /// </summary>
    public void OnInitialized()
    {
        Initialized?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Measures all drawing areas necessary to render the window components.
    /// </summary>
    public void Measure() => MeasureProvider.Measure();

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        var screenSize = Application.ScreenSize;

        var children = Children.ToList();
        var numberOfChildren = children.Count;
        var childrenSizeList = childrenSize.ToList();
        var maxRight = PixelUnit.Zero;
        var maxBottom = PixelUnit.Zero;

        for (var i = 0; i < numberOfChildren; i++)
        {
            var child = children[i];
            var childWrapSize = childrenSizeList[i];

            var childX = child.X.Value.IsAuto ? PixelUnit.Zero : child.X.Value;
            var childY = child.Y.Value.IsAuto ? PixelUnit.Zero : child.Y.Value;
            var childWidth = childWrapSize.Width;
            var childHeight = childWrapSize.Height;
            maxRight = Math.Max(maxRight, childX + childWidth);
            maxBottom = Math.Max(maxBottom, childY + childHeight);
        }

        maxRight = Math.Min(screenSize.Width, maxRight);
        maxBottom = Math.Min(screenSize.Height, maxBottom);

        return new Size(maxRight, maxBottom);
    }

    private void EventSystem_Render(IEventSystem sender, EventArgs e)
    {
        UpdateWindowAppearance();
        var renderingEngine = _renderingEngine;
        var canvas = new Canvas(renderingEngine, Application.LifeCycle);
        RenderProvider.Render(this, canvas, renderingEngine);
        renderingEngine.FinalizeFrame();
        canvas.LogStatistics(Application.Logger);
    }

    public void Shutdown()
    {
        Application.EventSystem.MousePress -= EventSystem_MousePress;
        // _windowManager.Shutdown(); // TODO: dispoable logic
    }

    private void EventSystem_MousePress(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnMousePress(windowArgs.Args);
        }
    }

    private void EventSystem_MouseRelease(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnMouseRelease(windowArgs.Args);
        }

    }

    private void EventSystem_MouseMove(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnMouseMove(windowArgs.Args);
        }
    }

    private void EventSystem_KeyPress(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnKeyPress(windowArgs.Args);
        }
    }

    private void EventSystem_KeyRelease(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnKeyRelease(windowArgs.Args);
        }
    }

    private void EventSystem_TextInput(object? sender, WindowEventArgs<TextInputEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnTextInput(windowArgs.Args);
        }
    }

    private void EventSystem_WindowMove(IEventSystem sender, WindowEventArgs<WindowMoveEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            var topLeft = windowArgs.Args.TopLeft;
            X.Value = topLeft.X;
            Y.Value = topLeft.Y;
            WindowMove?.Invoke(this, windowArgs.Args);
        }
    }

    private void EventSystem_WindowResize(IEventSystem sender, WindowEventArgs<WindowResizeEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            var size = windowArgs.Args.Size;
            Width.Value = size.Width;
            Height.Value = size.Height;
            WindowResize?.Invoke(this, windowArgs.Args);
        }
    }

    private void EventSystem_WindowOpen(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            WindowOpen?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EventSystem_WindowClose(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            WindowClose?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EventSystem_WindowMaximized(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            WindowMaximized?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EventSystem_WindowMinimized(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            WindowMinimized?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EventSystem_MouseWheel(IEventSystem sender, WindowEventArgs<MouseWheelEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowId))
        {
            OnMouseWheel(windowArgs.Args);
        }
    }

    private void Visibility_ValueChange(BindableProperty<ComponentVisibility> sender, ValueChangeEventArgs<ComponentVisibility> e)
    {
        if (Visibility.Value != ComponentVisibility.Visible)
        {
            Application.WindowManager.HideWindow(_windowId);
        }
        else
        {
            // TODO: Push to event queue
            _scheduleVisibilityChange = true;
        }
    }

    private void Window_RenderingAreaChange(UIComponent sender, RenderingAreaEventArgs e)
    {
        Application.WindowManager.SetRenderingArea(_windowId, e.RenderingArea);
        _renderingEngine.ViewportSize = e.RenderingArea.Size;
    }

    private void Window_WindowClose(Window sender, EventArgs e)
    {
        Visibility.Value = ComponentVisibility.Hidden;
        if (CloseBehavior.Value == WindowCloseBehavior.QuitApplication)
        {
            Application.Quit();
        }
    }

    private void UpdateWindowAppearance()
    {
        if (_scheduleVisibilityChange)
        {
            Application.WindowManager.ShowWindow(_windowId);
            _scheduleVisibilityChange = false;
        }

        Application.WindowManager.SetTitle(_windowId, Title.Value);
        Application.WindowManager.SetOpacity(_windowId, BackgroundColor.Value.AlphaComponent);
        Application.WindowManager.SetResizable(_windowId, Resizable.Value);
    }

    private void UpdateCloseBehavior()
    {
        switch (CloseBehavior.Value)
        {
            case WindowCloseBehavior.None:
                WindowClose -= Window_WindowClose;
                break;
            case WindowCloseBehavior.HideWindow:
                WindowClose += Window_WindowClose;
                break;
            case WindowCloseBehavior.QuitApplication:
                WindowClose += Window_WindowClose;
                break;
            default:
                throw new InvalidOperationException($"Unhandled enum {CloseBehavior.Value}");
        }
    }
}
