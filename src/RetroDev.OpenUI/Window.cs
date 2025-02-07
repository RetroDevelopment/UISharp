using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Components.Core;
using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI;

/// <summary>
/// Displays and manage a window.
/// </summary>
[EditorSettings(allow: false)]
public class Window : Container, IContainer
{
    internal readonly Invalidator _invalidator = new();
    private readonly IWindowManager _windowManager;
    private readonly MeasureProvider _measureProvider;
    private readonly RenderProvider _renderProvider;

    /// <summary>
    /// Raised when <see langword="this" /> <see cref="Window"/> has been initialized.
    /// This happens when all the initial <see cref="UIComponent"/> have been added to the window.
    /// </summary>
    public event TypeSafeEventHandler<Window, EventArgs> Initialized = (_, _) => { };

    /// <inheritdoc/>
    public override IEnumerable<UIComponent> Children => GetChildren();

    /// <summary>
    /// Creates a new window.
    /// </summary>
    /// <param name="application">The application owning this window.</param>
    /// <param name="windowManager">
    /// The optional <see cref="IWindowManager"/> that contains the implementation to actually render and manage windows.
    /// It can be injected to provide a custom implementation of window lifecycle (for example to use a specific window management framework) 
    /// or for unit testing.
    /// If not provided, the default OpenUI window manager will be used (it is not public).
    /// </param>
    public Window(Application application, IWindowManager? windowManager = null) : base(application, visibility: ComponentVisibility.Collapsed, isFocusable: true)
    {
        _windowManager = windowManager ?? new SDLWindowManager(application);
        _measureProvider = new MeasureProvider(this, _invalidator);
        _renderProvider = new RenderProvider(_invalidator);

        Application._eventSystem.Render += EventSystem_Render;
        application.AddWindow(this);
        Invalidate();

        BackgroundColor.BindDestinationToSource(Application.Theme.MainBackground);

        Visibility.ValueChange += Visibility_ValueChange;
        RenderingAreaChange += Window_RenderingAreaChange;

        application._eventSystem.MousePress += EventSystem_MousePress;
        application._eventSystem.MouseRelease += EventSystem_MouseRelease;
        application._eventSystem.MouseMove += EventSystem_MouseMove;
        application._eventSystem.KeyPress += EventSystem_KeyPress;
        application._eventSystem.KeyRelease += EventSystem_KeyRelease;
        application._eventSystem.TextInput += EventSystem_TextInput;
        application._eventSystem.WindowMove += EventSystem_WindowMove;
        application._eventSystem.WindowResize += EventSystem_WindowResize;
    }

    /// <summary>
    /// Adds a component to this window.
    /// </summary>
    /// <param name="component">The component to add.</param>
    public void AddComponent(UIComponent component)
    {
        AddChild(component);
    }

    /// <summary>
    /// Removes a component from this window.
    /// </summary>
    /// <param name="component">The component to remove.</param>
    /// <returns><see langword="true" /> if the component has been successfully removed, otherwise <see langword="false"/>
    /// (for example, if the component has never been added).
    /// </returns>
    public bool RemoveComponent(UIComponent component)
    {
        return RemoveChild(component);
    }

    /// <summary>
    /// Notifies the <see cref="Initialized"/> listeners that <see langword="this" /> window has been initialized.
    /// </summary>
    public void OnInitialized()
    {
        Initialized.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Measures all drawing areas necessary to render the window components.
    /// </summary>
    public void Measure() =>
        _measureProvider.Measure();

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        var screenSize = Application.ScreenSize;

        var children = Children.ToList();
        var numberOfChildren = children.Count;
        var childrenSizeList = childrenSize.ToList();
        var maxRight = PixelUnit.Zero;
        var maxBottom = PixelUnit.Zero;

        for (int i = 0; i < numberOfChildren; i++)
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
        var renderingEngine = _windowManager.RenderingEngine;
        var canvas = new Canvas(renderingEngine, Application.LifeCycle);
        _renderProvider.Render(this, canvas, renderingEngine);
        renderingEngine.FinalizeFrame();
        canvas.LogStatistics(Application.Logger);
    }

    public void Shutdown()
    {
        Application._eventSystem.MousePress -= EventSystem_MousePress;
        _windowManager.Shutdown();
    }

    private void EventSystem_MousePress(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnMousePress(windowArgs.Args);
        }
    }

    private void EventSystem_MouseRelease(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnMouseRelease(windowArgs.Args);
        }

    }

    private void EventSystem_MouseMove(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnMouseMove(windowArgs.Args);
        }
    }

    private void EventSystem_KeyPress(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnKeyPress(windowArgs.Args);
        }
    }

    private void EventSystem_KeyRelease(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnKeyRelease(windowArgs.Args);
        }
    }

    private void EventSystem_TextInput(object? sender, WindowEventArgs<TextInputEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnTextInput(windowArgs.Args);
        }
    }

    private void EventSystem_WindowMove(IEventSystem sender, WindowEventArgs<WindowMoveEventArgs> e)
    {
        var topLeft = e.Args.TopLeft;
        X.Value = topLeft.X;
        Y.Value = topLeft.Y;
    }

    private void EventSystem_WindowResize(IEventSystem sender, WindowEventArgs<WindowResizeEventArgs> e)
    {
        var size = e.Args.Size;
        Width.Value = size.Width;
        Height.Value = size.Height;
    }

    private void Visibility_ValueChange(Properties.BindableProperty<ComponentVisibility> sender, Properties.ValueChangeEventArgs<ComponentVisibility> e)
    {
        _windowManager.Visible = (e.CurrentValue == ComponentVisibility.Visible);
    }

    private void Window_RenderingAreaChange(UIComponent sender, RenderingAreaEventArgs e)
    {
        _windowManager.RenderingArea = e.RenderingArea;
        _windowManager.RenderingEngine.ViewportSize = e.RenderingArea.Size;
    }
}
