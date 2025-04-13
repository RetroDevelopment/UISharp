using RetroDev.UISharp.Components.Core;
using RetroDev.UISharp.Core.Contexts;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.OpenGL;
using RetroDev.UISharp.Core.Windowing;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Core.Windowing.SDL;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Themes;
using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.Windows;

/// <summary>
/// Displays and manage a window.
/// </summary>
// TODO: add disposable or shutdown method to call renderingEngine.Shutdown();
[EditorSettings(allow: false)]
public class Window : UISurface
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

    /// <summary>
    /// Indicates a special window status that alters the window rendering area.
    /// </summary>
    public enum WindowSizeStatus
    {
        /// <summary>
        /// The window occupies the rendering area size.
        /// </summary>
        None,

        /// <summary>
        /// The window is maximized and it occupies the full screen.
        /// </summary>
        Maximized,

        /// <summary>
        /// The window is minimized.
        /// </summary>
        Minimized
    }

    private readonly IWindowId _windowId;

    private Window? _modalChild;
    private Window? _modalOwner;

    /// <summary>
    /// Raised when <see langword="this" /> <see cref="Window"/> has been initialized.
    /// This happens when all the initial <see cref="UIObject"/> have been added to the window.
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
    /// Raised when the window manager requests that the window is closed.
    /// Note that this does not necessarily mean that the window will be closed (see <see cref="CloseBehavior"/>).
    /// If you want to be notified when a window is actually closed (meaning hidden) use the <see cref="UIObject.Visibility"/> <see cref="UIProperty{TValue}.ValueChange"/>
    /// event.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs>? WindowCloseRequest;

    /// <summary>
    /// The window title.
    /// </summary>
    public UIProperty<string> Title { get; }

    /// <summary>
    /// Whether the window is re-sizable.
    /// </summary>
    public UIProperty<bool> ReSizable { get; }

    /// <summary>
    /// The default action to execute when closing the window.
    /// By default, the first window created has the <see cref="WindowCloseBehavior.QuitApplication"/> and the subsequent
    /// windows have the <see cref="WindowCloseBehavior.HideWindow"/>.
    /// </summary>
    public UIProperty<WindowCloseBehavior> CloseBehavior { get; }

    /// <summary>
    /// The special status altering the window rendering area.
    /// </summary>
    public UIProperty<WindowSizeStatus> SizeStatus { get; }

    /// <summary>
    /// Whether the window is displayed in full screen.
    /// </summary>
    public UIProperty<bool> FullScreen { get; }

    /// <summary>
    /// A <see cref="Window"/> that acts as modal for <see langword="this" /> window.
    /// When this property is not <see langword="null" />, the assigned window is assumed to be modal
    /// and all window events directed to this <see langword="this" /> <see cref="Window"/> will be discarded.
    /// </summary>
    public Window? ModalChild
    {
        get
        {
            Application.Dispatcher.ThrowIfNotOnUIThread();
            return _modalChild;
        }
        set
        {
            Application.Dispatcher.ThrowIfNotOnUIThread();
            _modalChild = value;
        }
    }

    /// <summary>
    /// Creates a new window.
    /// </summary>
    /// <param name="application">The application owning this window.</param>
    /// <param name="renderingEngine">
    /// The rendering engine to render this window. By default the <see cref="OpenGLRenderingEngine"/> with <see cref="SDLOpenGLRenderingContext"/> is created.
    /// If you want to create another rendering engine (e.g. Vulkan) or you are not using the standard <see cref="SDLWindowManager"/>, you pass an instance of
    /// <see cref="IRenderingEngine"/>. If using OpenGL but not based on SDL, you can create a <see cref="OpenGLRenderingEngine"/> but you must implement and use your custom
    /// instance of <see cref="IRenderingContext"/>.
    /// </param>
    public Window(Application application, IRenderingEngine? renderingEngine = null) :
        base(application, visibility: ComponentVisibility.Collapsed, renderingEngine: renderingEngine, isFocusable: true, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();

        application.AddWindow(this);
        _windowId = Application.WindowManager.CreateWindow(RenderingEngine.RenderingContext);

        Title = new UIProperty<string>(this, string.Empty);
        ReSizable = new UIProperty<bool>(this, true);
        CloseBehavior = new UIProperty<WindowCloseBehavior>(this, application.Windows.Count() > 1 ? WindowCloseBehavior.HideWindow : WindowCloseBehavior.QuitApplication);
        SizeStatus = new UIProperty<WindowSizeStatus>(this, WindowSizeStatus.None);
        FullScreen = new UIProperty<bool>(this, false);

        //TODO: unregister in dispose?
        Title.ValueChange.Subscribe(title => Application.WindowManager.SetTitle(_windowId, title));
        ReSizable.ValueChange.Subscribe(resizable => Application.WindowManager.SetReSizable(_windowId, resizable));
        CloseBehavior.ValueChange.Subscribe(UpdateCloseBehavior);
        SizeStatus.ValueChange.Subscribe(UpdateSizeStatus);
        BackgroundColor.ValueChange.Subscribe(background => Application.WindowManager.SetOpacity(_windowId, background.AlphaComponent));
        Visibility.ValueChange.Subscribe(UpdateVisibility);
        Focus.ValueChange.Subscribe(UpdateFocus);
        FullScreen.ValueChange.Subscribe(UpdateFullScreen);
        MinimumWidth.ValueChange.Subscribe(_ => UpdateMinimumSize());
        MinimumHeight.ValueChange.Subscribe(_ => UpdateMinimumSize());
        MaximumWidth.ValueChange.Subscribe(_ => UpdateMaximumSize());
        MaximumHeight.ValueChange.Subscribe(_ => UpdateMaximumSize());

        Invalidate();

        BackgroundColor.BindTheme(UISharpColorNames.WindowBackground);

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
        application.EventSystem.WindowCloseRequest += EventSystem_WindowCloseRequest;
        application.EventSystem.WindowMaximized += EventSystem_WindowMaximized;
        application.EventSystem.WindowMinimized += EventSystem_WindowMinimized;
        application.EventSystem.WindowRestored += EventSystem_WindowRestored;
        application.EventSystem.WindowFocusGain += EventSystem_WindowFocusGain;
        application.EventSystem.WindowFocusLost += EventSystem_WindowFocusLost;
        // TODO remove events on dispose?
    }

    /// <summary>
    /// Resets the position (<see cref="X"/> and <see cref="Y"/>) and window size (<see cref="Width"/> and <see cref="Height"/>)
    /// to <see cref="PixelUnit"/> repositioning the window to auto size settings (<see cref="AutoWidth"/>, <see cref="HorizontalAlignment"/>, etc.).
    /// </summary>
    public void Reset()
    {
        X.Value = PixelUnit.Auto;
        Y.Value = PixelUnit.Auto;
        Width.Value = PixelUnit.Auto;
        Height.Value = PixelUnit.Auto;
    }

    private void UpdateMinimumSize()
    {
        Application.WindowManager.SetWindowMinimumSize(_windowId, new Size(MinimumWidth.Value, MinimumHeight.Value));
    }

    private void UpdateMaximumSize()
    {
        Application.WindowManager.SetWindowMaximumSize(_windowId, new Size(MaximumWidth.Value, MaximumHeight.Value));
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

    /// <summary>
    /// Prepares the window for the second pass layout.
    /// </summary>
    public void Prepare(bool allPasses) => MeasureProvider.Prepare(allPasses);

    /// <summary>
    /// Shows <see langword="this" /> <see cref="Window"/>.
    /// </summary>
    public void Show()
    {
        Visibility.Value = ComponentVisibility.Visible;
    }

    /// <summary>
    /// Shows <see langword="this" /> <see cref="Window"/> as modal.
    /// </summary>
    /// <param name="owner">The window for which <see langword="this"/> <see cref="Window"/> is modal.</param>
    public void ShowModal(Window owner)
    {
        owner.ModalChild = this;
        _modalOwner = owner;
        Show();
    }

    /// <summary>
    /// Closes <see langword="this" /> <see cref="Window"/>.
    /// </summary>
    public void Close()
    {
        Visibility.Value = ComponentVisibility.Hidden;
        if (_modalOwner is not null)
        {
            _modalOwner._modalChild = null;
            _modalOwner = null;
        }
    }

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
            var childWidth = childWrapSize.Width +
                             (child.Margin.Left.Value.IsAuto ? PixelUnit.Zero : child.Margin.Left.Value) +
                             (child.Margin.Right.Value.IsAuto ? PixelUnit.Zero : child.Margin.Right.Value);
            var childHeight = childWrapSize.Height +
                              (child.Margin.Top.Value.IsAuto ? PixelUnit.Zero : child.Margin.Top.Value) +
                              (child.Margin.Bottom.Value.IsAuto ? PixelUnit.Zero : child.Margin.Bottom.Value);
            maxRight = Math.Max(maxRight, childX + childWidth);
            maxBottom = Math.Max(maxBottom, childY + childHeight);
        }

        maxRight = Math.Min(screenSize.Width, maxRight);
        maxBottom = Math.Min(screenSize.Height, maxBottom);

        return new Size(maxRight, maxBottom);
    }

    internal void Render()
    {
        Invalidator.SelectCurrentInvalidatedItems(allPasses: true);
        var renderingEngine = RenderingEngine;
        RenderProvider.Render(this, renderingEngine);
        Invalidator.ResetAll();
    }

    internal void OnRenderDone()
    {
        CaptureActualPosition();
        CaptureActualSize();
    }

    public void Shutdown()
    {
        Application.EventSystem.MousePress -= EventSystem_MousePress;
        // _windowManager.Shutdown(); // TODO: dispoable logic
    }

    private void EventSystem_MousePress(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            var anyChildCapturedPressEvent = OnMousePress(windowArgs.Args);
            if (!anyChildCapturedPressEvent) Focus.Value = true;
        }
    }

    private void EventSystem_MouseRelease(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            OnMouseRelease(windowArgs.Args);
        }
    }

    private void EventSystem_MouseMove(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            OnMouseMove(windowArgs.Args);
        }
    }

    private void EventSystem_KeyPress(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            OnKeyPress(windowArgs.Args);
        }
    }

    private void EventSystem_KeyRelease(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            OnKeyRelease(windowArgs.Args);
        }
    }

    private void EventSystem_TextInput(object? sender, WindowEventArgs<TextInputEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            OnTextInput(windowArgs.Args);
        }
    }

    private void EventSystem_WindowMove(IEventSystem sender, WindowEventArgs<WindowMoveEventArgs> windowArgs)
    {
        var topLeft = windowArgs.Args.TopLeft;
        if (topLeft.X == X.Value && topLeft.Y == Y.Value) return;

        if (ShouldPropagateEvent(windowArgs) && !HasModal(windowArgs))
        {
            X.Value = topLeft.X;
            Y.Value = topLeft.Y;
            WindowMove?.Invoke(this, windowArgs.Args);
        }

        if (HasModal(windowArgs))
        {
            LockWindowArea(windowArgs.WindowId);
        }
    }

    private void EventSystem_WindowResize(IEventSystem sender, WindowEventArgs<WindowResizeEventArgs> windowArgs)
    {
        var size = windowArgs.Args.Size;
        if (Width.Value == size.Width && Height.Value == size.Height.Value) return;

        if (ShouldPropagateEvent(windowArgs))
        {
            Width.Value = size.Width;
            Height.Value = size.Height;
            WindowResize?.Invoke(this, windowArgs.Args);
        }

        if (HasModal(windowArgs))
        {
            LockWindowArea(windowArgs.WindowId);
        }
    }

    private void LockWindowArea(IWindowId windowId)
    {
        if (X.Value.IsAuto || Y.Value.IsAuto || Width.Value.IsAuto || Height.Value.IsAuto) return;
        var position = new Point(X.Value, Y.Value);
        var size = new Size(Width.Value, Height.Value);
        Application.WindowManager.SetWindowRenderingArea(windowId, new Area(position, size));
    }

    private void EventSystem_WindowOpen(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            WindowOpen?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EventSystem_WindowCloseRequest(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            WindowCloseRequest?.Invoke(this, EventArgs.Empty);
        }
    }

    private void EventSystem_WindowMaximized(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            SizeStatus.Value = WindowSizeStatus.Maximized;
        }
    }

    private void EventSystem_WindowMinimized(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            SizeStatus.Value = WindowSizeStatus.Minimized;
        }
    }

    private void EventSystem_WindowRestored(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            SizeStatus.Value = WindowSizeStatus.None;
        }
    }

    private void EventSystem_WindowFocusGain(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (HasModal(windowArgs))
        {
            Application.WindowManager.FocusWindow(_modalChild!._windowId);
        }

        if (ShouldPropagateEvent(windowArgs))
        {
            Focus.Value = true;
        }
    }

    private void EventSystem_WindowFocusLost(IEventSystem sender, WindowEventArgs<EventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            Focus.Value = false;
        }
    }


    private void EventSystem_MouseWheel(IEventSystem sender, WindowEventArgs<MouseWheelEventArgs> windowArgs)
    {
        if (ShouldPropagateEvent(windowArgs))
        {
            OnMouseWheel(windowArgs.Args);
        }
    }

    private void Window_RenderingAreaChange(UIObject sender, RenderingAreaEventArgs e)
    {
        Application.WindowManager.SetWindowRenderingArea(_windowId, e.RenderingArea);
        RenderingEngine.ViewportSize = e.RenderingArea.Size;
    }

    private void Window_WindowClose(Window sender, EventArgs e)
    {
        Visibility.Value = ComponentVisibility.Hidden;
        if (CloseBehavior.Value == WindowCloseBehavior.QuitApplication)
        {
            Application.Quit();
        }
    }

    private void UpdateSizeStatus(WindowSizeStatus sizeStatus)
    {
        switch (SizeStatus.Value)
        {
            case WindowSizeStatus.None:
                Application.WindowManager.RestoreWindow(_windowId);
                break;
            case WindowSizeStatus.Maximized:
                Application.WindowManager.Maximize(_windowId);
                break;
            case WindowSizeStatus.Minimized:
                Application.WindowManager.Minimize(_windowId);
                break;
            default:
                throw new InvalidOperationException($"Unrecognized enum type {SizeStatus.Value}");
        }
    }

    private void UpdateVisibility(ComponentVisibility visibility)
    {
        if (visibility != ComponentVisibility.Visible)
        {
            Application.WindowManager.HideWindow(_windowId);
        }
        else
        {
            Application.WindowManager.ShowWindow(_windowId);
        }
    }

    private void UpdateFocus(bool focus)
    {
        if (focus)
        {
            Application.WindowManager.FocusWindow(_windowId);
        }
        else
        {
            // TODO: remove focus? Maybe use focus group to set another window focus (similar to tab).
        }
    }

    private void UpdateFullScreen(bool fullScreen)
    {
        if (fullScreen)
        {
            Application.WindowManager.SetFullScreen(_windowId);
        }
        else
        {
            Application.WindowManager.RestoreFullScreen(_windowId);
        }
    }

    private void UpdateWindowAppearance()
    {
        Application.WindowManager.SetTitle(_windowId, Title.Value);
        Application.WindowManager.SetOpacity(_windowId, BackgroundColor.Value.AlphaComponent);
        Application.WindowManager.SetReSizable(_windowId, ReSizable.Value);

        switch (SizeStatus.Value)
        {
            case WindowSizeStatus.None:
                Application.WindowManager.RestoreWindow(_windowId);
                break;
            case WindowSizeStatus.Maximized:
                Application.WindowManager.Maximize(_windowId);
                break;
            case WindowSizeStatus.Minimized:
                Application.WindowManager.Minimize(_windowId);
                break;
            default:
                throw new InvalidOperationException($"Unrecognized enum type {SizeStatus.Value}");
        }

        if (FullScreen.Value)
        {
            Application.WindowManager.SetFullScreen(_windowId);
        }
        else
        {
            Application.WindowManager.RestoreFullScreen(_windowId);
        }
    }

    private void UpdateCloseBehavior(WindowCloseBehavior closeBehavior)
    {
        switch (closeBehavior)
        {
            case WindowCloseBehavior.None:
                WindowCloseRequest -= Window_WindowClose;
                break;
            case WindowCloseBehavior.HideWindow:
                WindowCloseRequest += Window_WindowClose;
                break;
            case WindowCloseBehavior.QuitApplication:
                WindowCloseRequest += Window_WindowClose;
                break;
            default:
                throw new InvalidOperationException($"Unhandled enum {CloseBehavior.Value}");
        }
    }

    private bool ShouldPropagateEvent<TEventArgs>(WindowEventArgs<TEventArgs> windowArgs) where TEventArgs : EventArgs =>
        windowArgs.WindowId.Equals(_windowId) && (_modalChild == null || _modalChild.Visibility.Value != ComponentVisibility.Visible);

    private bool HasModal<TEventArgs>(WindowEventArgs<TEventArgs> windowArgs) where TEventArgs : EventArgs =>
        windowArgs.WindowId.Equals(_windowId) && _modalChild is not null && _modalChild.Visibility.Value == ComponentVisibility.Visible;
}
