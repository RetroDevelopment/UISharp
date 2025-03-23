using RetroDev.UISharp.Components.Core;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Layout;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Exceptions;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Components.Base;

// TODO: Same way as before, add a TextResourceManager class to manage text (e.g. with language) and create it in Application.
// They can also have dictionary/xml. Then bind these properties in your project and you will get automatic language change.

/// <summary>
/// The abstract class for all UI components (windows, buttons, etc.).
/// </summary>
public abstract class UIComponent
{
    /// <summary>
    /// Defines visibility rules.
    /// </summary>
    public enum ComponentVisibility
    {
        /// <summary>
        /// The component is visible.
        /// </summary>
        Visible,

        /// <summary>
        /// The component is invisible but it still occupies the space in the UI, meaning that it preserves the drawing area.
        /// </summary>
        Hidden,

        /// <summary>
        /// The component is invisible and it does not occupy any space, meaning that the rendering area is zero.
        /// </summary>
        Collapsed
    }

    // The visual children, part of the actual hierarchy.
    // TODO: use a UIComponentCollection<UINode> to manage bindings etc.
    internal int _level = 0;
    private UIComponent? _focusedComponent;

    // Event information
    private bool _hasMouse = false;

    // Sizes
    private Size _wrapSize; // The size with auto size to wrap.
    private Area? _relativeDrawingAreaOverride = null; // Memorizes the latest parameter used in RecomputeDrawingArea()
    private Area _relativeDrawingArea; // Area relative to the parent. So (0, 0) is top left of parent.
    private Area _absoluteDrawingArea; // Area relative to the window. So (0, 0) is top left of window.
    private Area _clipArea; // Absolute clipping area. Each pixel with absolute coordinates outside of the area are clipped.

    /// <summary>
    /// Mouse button press inside <<see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs>? MousePress;

    /// <summary>
    /// Mouse button release inside <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs>? MouseRelease;

    /// <summary>
    /// Mouse position changed inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs>? MouseMove;

    /// <summary>
    /// Mouse has entered <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, EventArgs>? MouseEnter;

    /// <summary>
    /// Mouse has left <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, EventArgs>? MouseLeave;

    /// <summary>
    /// The mouse drag has started because the left click button has been pressed on
    /// <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    /// <seealso cref="MouseDrag"/>.
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs>? MouseDragBegin;

    /// <summary>
    /// The mouse is moving while the left button is being hold.
    /// Unlike the <see cref="MouseMove"/> event, this event is triggered even when the
    /// mouse is outside <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs>? MouseDrag;

    /// <summary>
    /// The mouse drag is over because the left mouse button has been released.
    /// </summary>
    /// <seealso cref="MouseDrag"/>.
    public event TypeSafeEventHandler<UIComponent, EventArgs>? MouseDragEnd;

    /// <summary>
    /// Key is pressed inside <see cref="this"/> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs>? KeyPress;

    /// <summary>
    /// Key is released inside <see langword="this"/> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs>? KeyRelease;

    /// <summary>
    /// Text is inserted in <see langword="this"/> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, TextInputEventArgs>? TextInput;

    /// <summary>
    /// Mouse wheel has been moved.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseWheelEventArgs>? MouseWheel;

    /// <summary>
    /// Triggered when <see langword="this" /> <see cref="UIComponent"/> drawing area changes.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingAreaEventArgs>? RenderingAreaChange;

    /// <summary>
    /// A frame need to be rendered. Use this event to render at the bottom of the children.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingEventArgs>? RenderFrame;

    /// <summary>
    /// The application in which <see langword="this"/> <see cref="UIComponent"/> runs.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The parent <see cref="UIComponent"/> containing <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public UIComponent? Parent { get; private set; }

    /// <summary>
    /// The drawing area of <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public Canvas Canvas { get; }

    /// <summary>
    /// Gets the <see cref="UIRoot"/> that contain <see langword="this" /> <see cref="UIComponent"/>.
    /// If <see langword="this" /> <see cref="UIComponent"/> has not been attached to a <see cref="UIRoot"/>,
    /// the value is <see langword="null" />.
    /// </summary>
    public UIRoot? Root => Parent?.Root ?? this as UIRoot;

    /// <summary>
    /// The actual component size as it was after the latest rendering.
    /// Inside <see cref="Application.SecondPassMeasure"/>, this is the expected component size before rendering the next fame.
    /// </summary>
    public Size ActualSize => _relativeDrawingArea.Size;

    /// <summary>
    /// The component unique identifier.
    /// </summary>
    public UIProperty<string> ID { get; }

    /// <summary>
    /// The component top-left corner X-coordinate in pixels.
    /// </summary>
    /// <remarks>The X-coordinate is relative to the parent component rendering area.</remarks>
    public UIProperty<PixelUnit> X { get; }

    /// <summary>
    /// The component top-left corner Y-coordinate in pixels.
    /// </summary>
    /// <remarks>The Y-coordinate is relative to the parent component rendering area.</remarks>
    public UIProperty<PixelUnit> Y { get; }

    /// <summary>
    /// The component width in pixels.
    /// </summary>
    public UIProperty<PixelUnit> Width { get; }

    /// <summary>
    /// The component height in pixels.
    /// </summary>
    public UIProperty<PixelUnit> Height { get; }

    /// <summary>
    /// Whether the component is rendered or not.
    /// </summary>
    public UIProperty<ComponentVisibility> Visibility { get; }

    /// <summary>
    /// Specifies how to automatically specify this component width.
    /// </summary>
    public UIProperty<IAutoSize> AutoWidth { get; }

    /// <summary>
    /// Specifies how to automatically specify this component height.
    /// </summary>
    public UIProperty<IAutoSize> AutoHeight { get; }

    /// <summary>
    /// Specifies the horizontal alignment of this component relative to its parent.
    /// </summary>
    public UIProperty<IHorizontalAlignment> HorizontalAlignment { get; }

    /// <summary>
    /// Specifies the vertical alignment of this component relative to its parent.
    /// </summary>
    public UIProperty<IVerticalAlignment> VerticalAlignment { get; }

    /// <summary>
    /// Whether this component can get focus.
    /// </summary>
    public UIProperty<bool> Focusable { get; }

    /// <summary>
    /// Whether this component has focus.
    /// </summary>
    public UIProperty<bool> Focus { get; }

    /// <summary>
    /// Whether this component is enabled and can receive events.
    /// </summary>
    public UIProperty<bool> Enabled { get; }

    /// <summary>
    /// The component background color.
    /// </summary>
    /// <remarks>
    /// It is the derived class responsibility to decide how to handle the background color.
    /// </remarks>
    public UIProperty<Color> BackgroundColor { get; }

    /// <summary>
    /// The <see cref="UIWidget"/> margin.
    /// </summary>
    public MarginGroup Margin { get; }

    /// <summary>
    /// The <see cref="UIWidget"/> padding.
    /// </summary>
    public PaddingGroup Padding { get; }

    /// <summary>
    /// The minimum width for <see langword="this" /> <see cref="UIComponent"/>.
    /// This property takes priority over the <see cref="Width"/>, meaning that even if
    /// <see cref="Width"/> is not <see cref="PixelUnit.Auto"/> the minimum width will still be <see cref="MinimumWidth"/>.
    /// </summary>
    public UIProperty<PixelUnit> MinimumWidth { get; }

    /// <summary>
    /// The minimum height for <see langword="this" /> <see cref="UIComponent"/>.
    /// This property takes priority over the <see cref="Height"/>, meaning that even if
    /// <see cref="Height"/> is not <see cref="PixelUnit.Auto"/> the minimum height will still be <see cref="MinimumHeight"/>.
    /// </summary>
    public UIProperty<PixelUnit> MinimumHeight { get; }

    /// <summary>
    /// The maximum width for <see langword="this" /> <see cref="UIComponent"/>.
    /// This property takes priority over the <see cref="Width"/>, meaning that even if
    /// <see cref="Width"/> is not <see cref="PixelUnit.Auto"/> the maximum width will still be <see cref="MaximumWidth"/>.
    /// </summary>
    public UIProperty<PixelUnit> MaximumWidth { get; }

    /// <summary>
    /// The maximum height for <see langword="this" /> <see cref="UIComponent"/>.
    /// This property takes priority over the <see cref="Height"/>, meaning that even if
    /// <see cref="Height"/> is not <see cref="PixelUnit.Auto"/> the maximum height will still be <see cref="MaximumHeight"/>.
    /// </summary>
    public UIProperty<PixelUnit> MaximumHeight { get; }

    /// <summary>
    /// The list of children of <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    protected UIPropertyCollection<UIWidget> Children { get; }

    /// <summary>
    /// Creates a new component.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="visibility">Whether the component is rendered or not.</param>
    /// <param name="isFocusable">Whether the component can get focus.</param>
    /// <param name="autoWidth">How to automatically determine this component width.</param>
    /// <param name="autoHeight">How to automatically determine this component height.</param>
    /// <param name="horizontalAlignment">The component horizontal alignment (relative to its <see cref="Parent"/>).</param>
    /// <param name="verticalAlignment">The component vertical alignment (relative to its <see cref="Parent"/>).</param>
    protected UIComponent(Application application,
                          ComponentVisibility visibility = ComponentVisibility.Visible,
                          bool isFocusable = true,
                          IAutoSize? autoWidth = null,
                          IAutoSize? autoHeight = null,
                          IHorizontalAlignment? horizontalAlignment = null,
                          IVerticalAlignment? verticalAlignment = null)
    {
        Application = application;
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();

        if (!Application.Started)
        {
            throw new UIInitializationException($"Cannot create UI element {this}: application must be started. Make sure all UI initialization is within the {nameof(Application.ApplicationStarted)} event");
        }

        ID = new UIProperty<string>(this, string.Empty);
        X = new UIProperty<PixelUnit>(this, PixelUnit.Auto);
        Y = new UIProperty<PixelUnit>(this, PixelUnit.Auto);
        Width = new UIProperty<PixelUnit>(this, PixelUnit.Auto);
        Height = new UIProperty<PixelUnit>(this, PixelUnit.Auto);
        Visibility = new UIProperty<ComponentVisibility>(this, visibility);
        AutoWidth = new UIProperty<IAutoSize>(this, autoWidth ?? AutoSize.Stretch);
        AutoHeight = new UIProperty<IAutoSize>(this, autoHeight ?? AutoSize.Stretch);
        HorizontalAlignment = new UIProperty<IHorizontalAlignment>(this, horizontalAlignment ?? Alignment.Center);
        VerticalAlignment = new UIProperty<IVerticalAlignment>(this, verticalAlignment ?? Alignment.Center);
        Focusable = new UIProperty<bool>(this, isFocusable);
        Focus = new UIProperty<bool>(this, false);
        Enabled = new UIProperty<bool>(this, true);
        BackgroundColor = new UIProperty<Color>(this, Color.Transparent);
        Margin = new MarginGroup(application, this);
        Padding = new PaddingGroup(application, this);
        MinimumWidth = new UIProperty<PixelUnit>(this, PixelUnit.Zero);
        MinimumHeight = new UIProperty<PixelUnit>(this, PixelUnit.Zero);
        MaximumWidth = new UIProperty<PixelUnit>(this, PixelUnit.Auto);
        MaximumHeight = new UIProperty<PixelUnit>(this, PixelUnit.Auto);

        Canvas = new Canvas(this);
        Children = new UIPropertyCollection<UIWidget>(application, lockChanges: true);
        Children.ValueAdd.Subscribe(OnChildAdd);
        Children.ValueRemove.Subscribe(OnChildRemove);

        _wrapSize = Size.Zero;
        _relativeDrawingArea = Area.Empty;
        _absoluteDrawingArea = Area.Empty;
        _clipArea = Area.Empty;

        Focus.ValueChange.Subscribe(OnFocusChange);
        Enabled.ValueChange.Subscribe(OnEnabledChange);

        UpdateVisibility();
        Visibility.ValueChange.Subscribe(v => UpdateVisibility());
    }

    /// <summary>
    /// Invalidates <see langword="this" /> <see cref="UIComponent"/>. An invalidate component is
    /// a components that needs to be redrawn.
    /// Invalidation is done automatically whenever a <see cref="UIProperty{TComponent, TValue}"/> changes,
    /// but it is possible to manually invalidate if needed.
    /// </summary>
    public void Invalidate()
    {
        Root?.Invalidator?.Invalidate(this);
    }

    /// <summary>
    /// Marks <see langword="this" /> <see cref="UIComponent"/> as no longer invalidated.
    /// It is usually not necessary to cancel invalidation because it is done automatically by the framework,
    /// but it is possible to manually override the behavior if needed.
    /// </summary>
    public void CancelInvalidation()
    {
        Root?.Invalidator?.CancelInvalidation(this);
    }

    /// <summary>
    /// Validates this component checking for inconsistencies.
    /// </summary>
    /// <exception cref="UIPropertyValidationException">
    /// If there is an inconsistency, for example, a <see cref="UIProperty{TComponent, TValue}"/> value out of range.
    /// </exception>
    /// <remarks>
    /// The validation is performed right before rendering, when re-calculating the component sizes. This allows for more flexibility
    /// when setting values that may temporarily lead to object inconsistency.
    /// However, since this method is public, validation can occur at any time.
    /// </remarks>
    public virtual void Validate()
    {
        if (!Width.Value.IsAuto && Width.Value < 0.0f) throw new UIPropertyValidationException($"Width must be greater or equal to zero, found {Width.Value}", this);
        if (!Height.Value.IsAuto && Height.Value < 0.0f) throw new UIPropertyValidationException($"Height must be greater or equal to zero, found {Height.Value}", this);
        if (!Focusable.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not focusable", this);
        if (!Enabled.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not enabled");
    }

    /// <summary>
    /// Captures the position of <see langword="this" /> <see cref="UIComponent"/> during the latest
    /// frame rendering or the position of the next frame rendering is this is called inside <see cref="Application.SecondPassMeasure"/> and
    /// writes the values into <see cref="X"/> and <see cref="Y"/>.
    /// </summary>
    /// <remarks>
    /// This method is useful when using drag-and-drop, because that usually implies that the <see cref="UIComponent"/> position is incremented or decremented as the object is dragged.
    /// Doing that when <see cref="X"/> and <see cref="Y"/> are set to <see cref="PixelUnit.Auto"/> would be impossible, so the first step is to capture the actual positions and then using 
    /// manual positioning.
    /// </remarks>
    public void CaptureActualPosition()
    {
        var topLeft = _relativeDrawingArea.TopLeft;
        X.Value = topLeft.X;
        Y.Value = topLeft.Y;
    }

    /// <summary>
    /// Captures the size of <see langword="this" /> <see cref="UIComponent"/> during the latest
    /// frame rendering or the position of the next frame rendering is this is called inside <see cref="Application.SecondPassMeasure"/> and
    /// writes the values into <see cref="Width"/> and <see cref="Height"/>.
    /// </summary>
    /// <remarks>
    /// This method is useful when using drag-and-drop, because that usually implies that the <see cref="UIComponent"/> size is incremented or decremented as the object is dragged.
    /// Doing that when <see cref="Width"/> and <see cref="Height"/> are set to <see cref="PixelUnit.Auto"/> would be impossible, so the first step is to capture the actual positions and then using 
    /// manual size.
    /// </remarks>
    public void CaptureActualSize()
    {
        var size = _relativeDrawingArea.Size;
        Width.Value = size.Width;
        Height.Value = size.Height;
    }

    /// <summary>
    /// Compute the minimum size necessary to display all the component correctly.
    /// It is used to calculate the optimal size in <see cref="AutoSize.Wrap"/> mode.
    /// This does NOT include padding, which is automatically taking into account by the framework when
    /// calculating the wrap size in <see cref="AutoSize.Wrap"/> mode.
    /// </summary>
    /// <param name="childrenSize">
    /// The best estimated size to render the children correctly.
    /// This parameter can be used for layout re-computation, where the best size is the size that fits
    /// all estimate sizes of the children.
    /// </param>
    /// <returns>The minimum size necessary to display all the component correctly.</returns>
    /// <remarks>
    /// Getting the minimum optimal size for a component might not be always easy. In that case, it should
    /// at least get close to it or be as small as possible.
    /// </remarks>
    protected abstract Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize);

    /// <summary>
    /// Override this method to establish custom final rendering area for <see langword="this" /> component children.
    /// This is useful when implementing layouts, where it is required to arrange layout children in a given area.
    /// </summary>
    /// <param name="availableSpace"><see langword="this" /> container full available size to render.</param>
    /// <param name="sizeHints">The children estimate sizes.</param>
    /// <returns>
    /// The rendering areas of all children of <see langword="this" /> component.
    /// If the list is empty, the default area calculation will be used, otherwise the list must have the same size
    /// of <paramref name="sizeHints"/>, that is, the same number of element of children.
    /// If an element of the list is <see langword="null" /> the respective child drawing area will be calculated automatically.
    /// If on element of a <see cref="Area"/> in the list (e.g. <see cref="Area.Size"/>.<see cref="Size.Width"/> is set to
    /// <see cref="PixelUnit.Auto"/>, default area calculation will be used.
    /// </returns>
    protected virtual List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> sizeHints) => [];

    protected void OnMousePress(MouseEventArgs mouseEventArgs)
    {
        MousePress?.Invoke(this, mouseEventArgs);
        if (mouseEventArgs.Button == MouseButton.Left)
        {
            MouseDragBegin?.Invoke(this, mouseEventArgs);
            Root?.GlobalEventInformation.MarkComponentAsDragged(this);
        }

        foreach (var child in Children.Reverse())
        {
            if (child.ShouldPropagateMouseEvent(mouseEventArgs.AbsoluteLocation))
            {
                child.OnMousePress(child.CreateEventWithRelativeLocation(mouseEventArgs));
                // The first child where to propagate this event will end the loop. This avoid event propagation on multiple overlapping children.
                // The first child where the event is propagated is also the latest in the list, meaning the one rendered on top.
                break;
            }
        }
    }

    protected void OnMouseRelease(MouseEventArgs mouseEventArgs)
    {
        MouseRelease?.Invoke(this, mouseEventArgs);

        foreach (var child in Children.Reverse())
        {
            if (child.ShouldPropagateMouseEvent(mouseEventArgs.AbsoluteLocation))
            {
                child.OnMouseRelease(child.CreateEventWithRelativeLocation(mouseEventArgs));
                // The first child where to propagate this event will end the loop. This avoid event propagation on multiple overlapping children.
                // The first child where the event is propagated is also the latest in the list, meaning the one rendered on top.
                break;
            }
        }
    }

    protected void OnMouseMove(MouseEventArgs mouseEventArgs)
    {
        MouseMove?.Invoke(this, mouseEventArgs);

        var childHit = false;

        foreach (var child in Children.Reverse())
        {
            if (child.ShouldPropagateMouseEvent(mouseEventArgs.AbsoluteLocation) && !childHit)
            {
                child.NotifyMouseInside();
                child.OnMouseMove(child.CreateEventWithRelativeLocation(mouseEventArgs));
                // The first child where to propagate this event will end the loop. This avoid event propagation on multiple overlapping children.
                // The first child where the event is propagated is also the latest in the list, meaning the one rendered on top.
                childHit = true;
            }
            else
            {
                child.NotifyMouseOutside();
            }
        }
    }

    protected internal void OnMouseDrag(MouseEventArgs mouseEventArgs)
    {
        MouseDrag?.Invoke(this, mouseEventArgs);
    }

    protected internal void OnMouseDragEnd()
    {
        MouseDragEnd?.Invoke(this, EventArgs.Empty);
    }

    protected void OnKeyPress(KeyEventArgs keyEventArgs)
    {
        // TODO: looping through all the tree is unnecessary. Just detect the focused elements in the list and trigger events for that
        KeyPress?.Invoke(this, keyEventArgs);

        foreach (var child in Children)
        {
            if (child.Visibility.Value == ComponentVisibility.Visible && (child.Focus.Value || !child.Focusable.Value))
            {
                child.OnKeyPress(keyEventArgs);
            }
        }
    }

    protected void OnKeyRelease(KeyEventArgs keyEventArgs)
    {
        // TODO: looping through all the tree is unnecessary. Just detect the focused elements in the list and trigger events for that
        KeyRelease?.Invoke(this, keyEventArgs);

        foreach (var child in Children)
        {
            if (child.Visibility.Value == ComponentVisibility.Visible && (child.Focus.Value || !child.Focusable.Value))
            {
                child.OnKeyRelease(keyEventArgs);
            }
        }
    }

    protected void OnTextInput(TextInputEventArgs textInputEventArgs)
    {
        // TODO: looping through all the tree is unnecessary. Just detect the focused elements in the list and trigger events for that
        TextInput?.Invoke(this, textInputEventArgs);

        foreach (var child in Children)
        {
            if (child.Visibility.Value == ComponentVisibility.Visible && (child.Focus.Value || !child.Focusable.Value))
            {
                child.OnTextInput(textInputEventArgs);
            }
        }
    }

    protected void OnMouseWheel(MouseWheelEventArgs mouseWheelEventArgs)
    {
        MouseWheel?.Invoke(this, mouseWheelEventArgs);

        foreach (var child in Children.Reverse())
        {
            if (child.ShouldPropagateMouseEvent(mouseWheelEventArgs.AbsoluteLocation))
            {
                var childEventArgs = new MouseWheelEventArgs(mouseWheelEventArgs.HorizontalMovement,
                                                             mouseWheelEventArgs.VerticalMovement,
                                                             mouseWheelEventArgs.AbsoluteLocation,
                                                             mouseWheelEventArgs.AbsoluteLocation - child._absoluteDrawingArea.TopLeft);

                // The first child where to propagate this event will end the loop. This avoid event propagation on multiple overlapping children.
                // The first child where the event is propagated is also the latest in the list, meaning the one rendered on top.
                child.OnMouseWheel(childEventArgs);
                break;
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="UIProperty{TValue}"/> and binds it to the theme color with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="id">The theme color id to bind.</param>
    /// <returns>The <see cref="UIProperty{TComponent, TValue}"/> bound to the theme color with the given <paramref name="id"/>.</returns>
    /// <exception cref="ArgumentException">If a color with the given <paramref name="id"/> does not exist in the current theme.</exception>
    /// <typeparam name="TComponent">The component type.</typeparam>
    protected UIProperty<Color> CreateNewColorPropertyFor<TComponent>(string id) where TComponent : UIComponent
    {
        var themeProperty = Application.ThemeManager.GetColorProperty(id);
        return new UIProperty<Color>(this, themeProperty, BindingType.SourceToDestination);
    }

    internal IEnumerable<UIComponent> GetComponentTreeNodesDepthFirstSearch() =>
        Children.Union(Children.SelectMany(c => c.GetComponentTreeNodesDepthFirstSearch()));

    internal void OnRenderFrame()
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        Application.LifeCycle.ThrowIfNotOnRenderingPhase();

        RenderFrame?.Invoke(this, new RenderingEventArgs(_relativeDrawingArea.Size));
        Canvas.ContainerAbsoluteDrawingArea = _absoluteDrawingArea;
        Canvas.Render(_clipArea);
    }

    /// <summary>
    /// Computes the size of the component if <see cref="AutoSize.Wrap"/> is chose for both width and hight.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the size has changed since the last time this method was called, otherwise <see langword="false" />.
    /// </returns>
    internal bool ReComputeWrapSize()
    {
        var childrenSize = Children.Select(c => c._wrapSize);
        var minimalOptimalSize = ComputeMinimumOptimalSize(childrenSize);
        var width = Width.Value.IfAuto(minimalOptimalSize.Width);
        var height = Height.Value.IfAuto(minimalOptimalSize.Height);
        var collapsed = Visibility.Value == ComponentVisibility.Collapsed;
        var currentWrapSize = _wrapSize;
        var newWrapSize = collapsed ? Size.Zero : new Size(width, height);
        var minimumSize = new Size(MinimumWidth.Value, MinimumHeight.Value);
        var maximumSize = new Size(MaximumWidth.Value, Height.Value);
        newWrapSize = newWrapSize.Inflate(Padding.ToMarginStruct()).Clamp(minimumSize, maximumSize);
        _wrapSize = newWrapSize;
        return currentWrapSize != newWrapSize;
    }

    /// <summary>
    /// Computed the final component rendering area.
    /// </summary>
    /// <param name="relativeDrawingArea">A way to override the rendering area calculated values.</param>
    /// <param name="rootCall">Whether this is called from <see cref="MeasureProvider"/> and it requires to perform the invalidated subtree area re-calculation.</param>
    /// <exception cref="InvalidOperationException">
    /// If <see cref="RepositionChildren(Size, IEnumerable{Size})"/> return list is not empty and it has not the same size as <see cref="_childNodes"/>.
    /// </exception>
    internal void ComputeDrawingAreas(Area? relativeDrawingArea = null, bool rootCall = false)
    {
        bool renderingAreaChanged = false;
        if (!rootCall) _relativeDrawingAreaOverride = relativeDrawingArea;
        _relativeDrawingArea = ComputeRelativeDrawingArea(ref renderingAreaChanged, _relativeDrawingAreaOverride);
        _absoluteDrawingArea = ComputeAbsoluteDrawingArea(ref renderingAreaChanged);
        _clipArea = ComputeClipArea(ref renderingAreaChanged);

        var childrenAreas = RepositionChildren(_relativeDrawingArea.Size, Children.Select(c => c._wrapSize));

        // If rendering area has not changed and children area has not changed, no need to proceed.
        if (childrenAreas.Count == 0 && !renderingAreaChanged)
        {
            Validate();
            return;
        }

        if (childrenAreas.Count != 0 && Children.Count != 0 && childrenAreas.Count != Children.Count)
        {
            throw new InvalidOperationException($"{nameof(RepositionChildren)} must return the same number of elements as the number of children or be empty: {childrenAreas.Count()} provided but {Children.Count} exist");
        }

        // If rendering area has changed, ensure the component is invalidated.
        Invalidate();

        if (childrenAreas.Count != 0)
        {
            for (var i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var childArea = childrenAreas[i];
                child.ComputeDrawingAreas(childArea);
            }
        }
        else
        {
            foreach (var child in Children)
            {
                child.ComputeDrawingAreas();
            }
        }

        Validate();
        RenderingAreaChange?.Invoke(this, new RenderingAreaEventArgs(_relativeDrawingArea));
    }

    internal MouseEventArgs CreateEventWithRelativeLocation(MouseEventArgs mouseEventArgs) =>
        new(mouseEventArgs.AbsoluteLocation,
            mouseEventArgs.AbsoluteLocation - _absoluteDrawingArea.TopLeft,
            mouseEventArgs.Button,
            mouseEventArgs.Clicks);

    internal uint UpdateZIndices(uint baseZIndex)
    {
        // TODO: optimize traversal only for affected z indexes!
        var currentZIndex = Canvas.UpdateZIndices(baseZIndex);
        foreach (var child in Children)
        {
            currentZIndex = child.UpdateZIndices(currentZIndex);
        }

        return currentZIndex;
    }

    private void OnFocusChange(bool focus)
    {
        if (focus)
        {
            RequestFocusFor(this);
        }
    }

    private void OnEnabledChange(bool enabled)
    {
        if (!enabled)
        {
            Focus.Value = false;
        }
    }

    // Ensure that only one child component has focus.
    private void RequestFocusFor(UIComponent component)
    {
        if (Root == null) throw new InvalidOperationException("Cannot request focus for a component not attached to a window");

        // Only the root component can manage focus, because only one object can be focusable at a time in a window.
        // TODO: When implementing focus groups, just change the logic here to not delegate this to the parent.
        if (Parent != null)
        {
            Root?.RequestFocusFor(component);
            return;
        }

        if (_focusedComponent != null)
        {
            _focusedComponent.Focus.Value = false;
        }

        _focusedComponent = component;
    }

    private Area ComputeRelativeDrawingArea(ref bool changed, Area? areaOverride = null)
    {
        if (Visibility.Value == ComponentVisibility.Collapsed) return Area.Empty;
        var sizeOverride = areaOverride?.Size ?? new Size(PixelUnit.Auto, PixelUnit.Auto);
        var locationOverride = areaOverride?.TopLeft ?? new Point(PixelUnit.Auto, PixelUnit.Auto);
        var parentSize = Parent?._relativeDrawingArea?.Size ?? Application.ScreenSize;

        var autoWidth = AutoWidth.Value.ComputeWidth(parentSize, _wrapSize);
        var autoHeight = AutoHeight.Value.ComputeHeight(parentSize, _wrapSize);
        var minimumSize = new Size(MinimumWidth.Value, MinimumHeight.Value);
        var maximumSize = new Size(MaximumWidth.Value, MaximumHeight.Value);
        var autoSize = new Size(autoWidth, autoHeight);
        var actualWidth = sizeOverride.Width.IsAuto ? (Width.Value.IsAuto ? autoSize.Width : Width.Value) : sizeOverride.Width;
        var actualHeight = sizeOverride.Height.IsAuto ? (Height.Value.IsAuto ? autoSize.Height : Height.Value) : sizeOverride.Height;
        var actualSize = new Size(actualWidth, actualHeight).Clamp(minimumSize, maximumSize);

        var autoX = HorizontalAlignment.Value.ComputeX(parentSize, actualSize);
        var autoY = VerticalAlignment.Value.ComputeY(parentSize, actualSize);
        var actualX = locationOverride.X.IsAuto ? (X.Value.IsAuto ? autoX : X.Value) : locationOverride.X;
        var actualY = locationOverride.Y.IsAuto ? (Y.Value.IsAuto ? autoY : Y.Value) : locationOverride.Y;
        var actualTopLeft = new Point(actualX, actualY);

        // Windows X and Y position will be relative to the screen, but the relative area location is Point.Zero, because it is relative to the viewport (i.e. the window itself).
        var margin = Margin.ToMarginStruct();
        var finalArea = new Area(actualTopLeft, actualSize);
        var clampedArea = finalArea.Clamp(parentSize, margin);
        // The rendering area is the finalArea calculated, clamped only for auto values. For example if X is manually set, that will override clamping in order to avoid
        // for example a component with X = 0 being positioned somewhere else because of margins. Manual overrides take priority over margin re-positioning.
        var result = new Area(new Point(locationOverride.X.IsAuto && X.Value.IsAuto ? clampedArea.TopLeft.X : finalArea.TopLeft.X,
                                        locationOverride.Y.IsAuto && Y.Value.IsAuto ? clampedArea.TopLeft.Y : finalArea.TopLeft.Y),
                              new Size(sizeOverride.Width.IsAuto && Width.Value.IsAuto ? clampedArea.Size.Width : finalArea.Size.Width,
                                       sizeOverride.Height.IsAuto && Height.Value.IsAuto ? clampedArea.Size.Height : finalArea.Size.Height));
        changed |= result != _relativeDrawingArea;
        return result;
    }

    private Area ComputeAbsoluteDrawingArea(ref bool changed)
    {
        Area result;
        if (Parent != null)
        {
            result = _relativeDrawingArea.ToAbsolute(Parent._absoluteDrawingArea);
        }
        else
        {
            result = _relativeDrawingArea.Fill();
        }
        changed |= result != _absoluteDrawingArea;
        return result;
    }

    private Area ComputeClipArea(ref bool changed)
    {
        var result = _absoluteDrawingArea.Clip(Parent?._clipArea);
        changed |= result != _clipArea;
        return result;
    }

    // Recompute the level of this component in the UI hierarchy tree.
    private void RecomputeLevel()
    {
        CancelInvalidation(); // Invalidation is based on _level, which will be wrong if kept where it is. A re-invalidation will be necessary.

        if (Parent != null && Root != null)
        {
            _level = Parent._level + 1;
        }
        else
        {
            _level = 0;
        }

        foreach (var child in Children) child.RecomputeLevel();
    }

    private void InvalidateAll()
    {
        if (Root == null) return;
        Invalidate();
        foreach (var child in Children) child.InvalidateAll();
    }

    private void CancelInvalidationAll()
    {
        if (Root == null) return;
        CancelInvalidation();
        foreach (var child in Children) child.CancelInvalidationAll();
    }

    private void AttachCanvas()
    {
        var renderingEngine = Root?.RenderingEngine;
        if (renderingEngine != null)
        {
            Canvas.Attach(renderingEngine);
            foreach (var child in Children)
            {
                child.AttachCanvas();
            }
        }
    }

    private void Cleanup()
    {
        Canvas.Detach();
        CancelInvalidation();
        foreach (var child in Children) child.Cleanup();
    }

    private void UpdateVisibility(bool? canInvalidatedParentRender = null)
    {
        var canRender = Visibility.Value == ComponentVisibility.Visible;
        if (canInvalidatedParentRender != null) canRender &= canInvalidatedParentRender.Value;
        Invalidate();
        Canvas.UpdateVisibility(canRender);
        foreach (var child in Children) child.UpdateVisibility(canRender);
    }

    private void NotifyMouseInside()
    {
        if (!_hasMouse)
        {
            _hasMouse = true;
            MouseEnter?.Invoke(this, EventArgs.Empty);
        }
    }

    private void NotifyMouseOutside()
    {
        if (_hasMouse)
        {
            _hasMouse = false;
            MouseLeave?.Invoke(this, EventArgs.Empty);
            foreach (var child in Children) child.NotifyMouseOutside();
        }
    }

    private bool ShouldPropagateMouseEvent(Point mouseAbsoluteLocation) =>
        mouseAbsoluteLocation.IsWithin(_absoluteDrawingArea) && Visibility.Value == ComponentVisibility.Visible && Enabled.Value;

    private void OnChildAdd(int index)
    {
        var component = Children[index];
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        if (Root != null) Root.Invalidator.NeedZIndexUpdate = true;
        component.Parent?.Children.Remove(component);
        component.Parent = this;
        component.RecomputeLevel();
        component.InvalidateAll();
        component.AttachCanvas();
        Invalidate();
        UpdateVisibility();
    }

    private void OnChildRemove(int index)
    {
        var component = Children[index];
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        if (Root != null) Root.Invalidator.NeedZIndexUpdate = true;
        Invalidate();

        if (component.Parent == this)
        {
            component.Cleanup();
            component.Parent = null;
        }
    }
}
