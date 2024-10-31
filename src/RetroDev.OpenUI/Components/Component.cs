using System.Runtime.CompilerServices;
using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Events;
using RetroDev.OpenUI.Core.Internal;

namespace RetroDev.OpenUI.Components;

/// <summary>
/// The abstract calss for all UI components (windows, buttons, etc.).
/// </summary>
public abstract class Component
{
    internal readonly IWindowManager _windowManager;
    internal readonly IRenderingEngine _renderingEngine;
    internal readonly ISVGRenderingEngine _svgEngine;
    internal readonly IFontRenderingEngine _fontEngine;

    private readonly Canvas _canvas;
    private readonly List<Component> _children = [];
    private readonly Component? _parent;

    /// <summary>
    /// Triggered when the <see cref="X"/> or <see cref="Y"/> location coordinates change.
    /// </summary>
    public event EventHandler LocationChanged = (_, _) => { };

    /// <summary>
    /// Triggered when the <see cref="Width"/> or <see cref="Height"/> component size change.
    /// </summary>
    public event EventHandler SizeChanged = (_, _) => { };

    /// <summary>
    /// When the component changes visiblity.
    /// </summary>
    public event EventHandler VisibilityChange = (_, _) => { };

    /// <summary>
    /// Mouse button press inside <see cref="this"/> window.
    /// </summary>
    public event EventHandler<MouseEventArgs> MousePress = (_, _) => { };

    /// <summary>
    /// Mouse button release inside <see cref="this"/> window.
    /// </summary>
    public event EventHandler<MouseEventArgs> MouseRelease = (_, _) => { };

    /// <summary>
    /// Mouse position changed inside <see cref="this"/> window.
    /// </summary>
    public event EventHandler<MouseEventArgs> MouseMove = (_, _) => { };

    /// <summary>
    /// Key is pressed inside <see cref="this"/> window.
    /// </summary>
    public event EventHandler<KeyEventArgs> KeyPress = (_, _) => { };

    /// <summary>
    /// Key is released inside <see cref="this"/> window.
    /// </summary>
    public event EventHandler<KeyEventArgs> KeyRelease = (_, _) => { };

    /// <summary>
    /// The component top-left corner X-coordinate in pixels.
    /// </summary>
    /// <remarks>The X-coordinate is relative to the parent component rendering area.</remarks>
    public int X
    {
        get => GetNonNullableProperty<int>(0);
        set => SetProperty(value, LocationChanged);
    }

    /// <summary>
    /// The component top-left corner Y-coordinate in pixels.
    /// </summary>
    /// <remarks>The Y-coordinate is relative to the parent component rendering area.</remarks>
    public int Y
    {
        get => GetNonNullableProperty<int>(0);
        set => SetProperty(value, LocationChanged);
    }

    /// <summary>
    /// The component width in pixels.
    /// </summary>
    public int Width
    {
        get => GetNonNullableProperty<int>(0);
        set => SetProperty(value, SizeChanged);
    }

    /// <summary>
    /// The component height in pixels.
    /// </summary>
    public int Height
    {
        get => GetNonNullableProperty<int>(0);
        set => SetProperty(value, SizeChanged);
    }

    /// <summary>
    /// Whether the component is rendered or not.
    /// </summary>
    public bool Visible
    {
        get => GetNonNullableProperty(false);
        set => SetProperty(value, VisibilityChange);
    }

    /// <summary>
    /// The 2D area (in pixels) where this component is rendered. The area is relative to the parent area,
    /// so [(0, 0), (100, 100)] would indicate that the component is rendered at the top-left of the paraent component,
    /// and it has size 100x100 pixels.
    /// </summary>
    public Area RelativeDrawingArea => new(new(X, Y), new(Width, Height));

    /// <summary>
    /// The 2D area (in pixels) where this component is rendered. The area is absolute to the window and it is clipped
    /// so that it doesn't go out of the parent boundaries, if <see langword="this"/> component has a parent.
    /// So, if this component <see cref="RelativeDrawingArea"/> is [(10, 10), (100, 100)], and the parent <see cref="AbsoluteDrawingArea"/>
    /// is [(20, 20), (200, 200)], the <see cref="RelativeDrawingArea"/> of <see langword="this"/> component will be [(30, 30), (100, 100)].
    /// </summary>
    /// <remarks>
    /// The absolute drawing area is clipped so that it doesn't go out of the parent drawing area. Clipping is done by resizing
    /// the area.
    /// </remarks>
    public Area AbsoluteDrawingArea
    {
        get
        {
            var absoluteDrawingAreaTopLeft = (_parent != null ? RelativeDrawingArea.TopLeft : Point.ZERO) + RelativeDrawingArea.TopLeft;
            var absoluteDrawingArea = new Area(absoluteDrawingAreaTopLeft, RelativeDrawingArea.Size);
            absoluteDrawingArea = absoluteDrawingArea.Clip(_parent?.AbsoluteDrawingArea);
            return absoluteDrawingArea;
        }
    }

    /// <summary>
    /// The root component owning all the component in the rendering area <see langword="this"/> component is located.
    /// It is typically the <see cref="Window"/>. If <see cref="this"/> component is the root itself, then <see cref="Root"/> is
    /// equal to <see langword="this"/>.
    /// </summary>
    protected Component Root { get; }

    internal PropertyManager PropertyManager { get; set; }
    internal LifeCycle LifeCycle { get; }

    /// <summary>
    /// Creates a new component.
    /// </summary>
    /// <param name="parent">The parent component that contains <see cref="this"/> component.</param>
    protected Component(Component parent) : this(parent.LifeCycle, parent._windowManager, parent._svgEngine, parent._fontEngine)
    {
        parent._children.Add(this);
        X = parent.X;
        Y = parent.Y;
        Width = parent.Width;
        Height = parent.Height;
        _parent = parent;
        Root = parent.Root;
        PropagateEventsFromParent();
    }

    internal Component(LifeCycle lifeCycle, IWindowManager windowManager, ISVGRenderingEngine svgEngine, IFontRenderingEngine fontEngine)
    {
        lifeCycle.ThrowIfNotOnUIThread();
        lifeCycle.ThrowIfPropertyCannotBeSet();

        LifeCycle = lifeCycle;
        PropertyManager = new PropertyManager(lifeCycle, this);
        Root = this;
        _windowManager = windowManager;
        _renderingEngine = windowManager.RenderingEngine;
        _svgEngine = svgEngine;
        _fontEngine = fontEngine;
        _canvas = new Canvas(_renderingEngine, _svgEngine, _fontEngine, LifeCycle);
        _parent = null;
    }

    /// <summary>
    /// Renders the component.
    /// </summary>
    /// <param name="canvas">The object that allows drawing into the drawing area.</param>
    protected abstract void Render(Canvas canvas);

    /// <summary>
    /// Renders the component.
    /// </summary>
    protected internal virtual void Render()
    {
        LifeCycle.ThrowIfNotOnUIThread();
        LifeCycle.ThrowIfNotOnRenderingPhase();

        _canvas.AbsoluteDrawingArea = AbsoluteDrawingArea;
        if (!Visible) return;
        Render(_canvas);
        _children.ForEach((child) => child.Render());
    }

    protected internal virtual void OnLocationChanged(object sender)
    {
        LocationChanged?.Invoke(sender, EventArgs.Empty);
    }

    protected internal virtual void OnSizeChanged(object sender)
    {
        SizeChanged?.Invoke(sender, EventArgs.Empty);
    }

    protected internal virtual void OnVisibilityChange(object sender)
    {
        VisibilityChange?.Invoke(sender, EventArgs.Empty);
    }

    protected internal virtual void OnMousePress(object sender, MouseEventArgs e)
    {
        MousePress?.Invoke(sender, e);
    }

    protected internal virtual void OnMouseRelease(object sender, MouseEventArgs e)
    {
        MouseRelease?.Invoke(sender, e);
    }

    protected internal virtual void OnMouseMove(object sender, MouseEventArgs e)
    {
        MouseMove?.Invoke(sender, e);
    }

    protected internal virtual void OnKeyPress(object sender, KeyEventArgs e)
    {
        KeyPress?.Invoke(sender, e);
    }

    protected internal virtual void OnKeyRelease(object sender, KeyEventArgs e)
    {
        KeyRelease?.Invoke(sender, e);
    }

    /// <summary>
    /// Sets a property and notifies the corresponding property change event.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="propertyValue">The property value to set.</param>
    /// <param name="eventCallback">The property change callback.</param>
    /// <param name="propertyName">The property name. Omitting this parameter in a setter will set it to the setter name.</param>
    /// <exception cref="InvalidUIThreadException">If invoking this method from a non UI thread.</exception>
    protected void SetProperty<T>(T propertyValue, EventHandler eventCallback, [CallerMemberName] string propertyName = "") =>
        PropertyManager.SetProperty(propertyValue, eventCallback, propertyName);

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="propertyName">The property name. Omitting this parameter in a setter will set it to the getter name.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="InvalidUIThreadException">If invoking this method from a non UI thread.</exception>
    protected T? GetNullableProperty<T>([CallerMemberName] string propertyName = "") =>
        PropertyManager.GetNullableProperty<T>(propertyName);

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="propertyName">The property name. Omitting this parameter in a setter will set it to the getter name.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="InvalidUIThreadException">If invoking this method from a non UI thread.</exception>
    protected T GetNonNullableProperty<T>(T defaultValue, [CallerMemberName] string propertyName = "") =>
        PropertyManager.GetNonNullableProperty<T>(defaultValue, propertyName);

    private void PropagateEventsFromParent()
    {
        if (_parent == null) return;

        _parent.MousePress += (sender, mouseEventArgs) =>
        {
            if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea))
            {
                MousePress.Invoke(this, new(mouseEventArgs.AbsoluteLocation,
                                            mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                            mouseEventArgs.Button));
            }
        };

        _parent.MouseRelease += (sender, mouseEventArgs) =>
        {
            if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea))
            {
                MouseRelease.Invoke(this, new(mouseEventArgs.AbsoluteLocation,
                                            mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                            mouseEventArgs.Button));
            }
        };

        _parent.MouseMove += (sender, mouseEventArgs) =>
        {
            if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea))
            {
                MouseMove.Invoke(this, new(mouseEventArgs.AbsoluteLocation,
                                           mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                           mouseEventArgs.Button));
            }
        };

        _parent.KeyPress += (sender, keyEventArgs) =>
        {
            KeyPress.Invoke(this, new(keyEventArgs.Button));
        };

        _parent.KeyRelease += (sender, keyEventArgs) =>
        {
            KeyRelease.Invoke(this, new(keyEventArgs.Button));
        };
    }
}
