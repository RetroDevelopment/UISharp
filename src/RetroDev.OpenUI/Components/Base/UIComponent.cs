using RetroDev.OpenUI.Components.Core;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;

namespace RetroDev.OpenUI.Components.Base;

// TODO: Same way as before, add a TextResourceManager class to manage text (e.g. with language) and create it in Application.
// They can also have dictionary/xml. Then bind these properties in your project and you will get automatic language change.

/// <summary>
/// The abstract calss for all UI components (windows, buttons, etc.).
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
    internal readonly List<UIWidget> _childNodes = [];
    internal int _level = 0;
    private UIComponent? _focusedComponent;
    private Point? _mouseAbsoluteLocation = null;
    private Point? _mouseDragPointAbsolute = null;
    private Point? _mouseLastDragPointAbsolute = null;
    private Size _wrapSize; // The size with auto size to wrap.
    private Area? _relativeDrawingAreaOverride = null; // Memorizes the latest parameter used in RecomputeDrawingArea()
    private Area _relativeDrawingArea; // Area relative to the parent. So (0, 0) is top left of parent.
    private Area _absoluteDrawingArea; // Area relative to the window. So (0, 0) is top left of window.
    private Area _clipArea; // Absolute clipping area. Each pixel with absolute cooridnates outside of the area are clipped.

    /// <summary>
    /// Mouse button press inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MousePress = (_, _) => { };

    /// <summary>
    /// Mouse button release inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseRelease = (_, _) => { };

    /// <summary>
    /// Mouse position changed inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseMove = (_, _) => { };

    /// <summary>
    /// Mouse dragging. This means that a left click has happend whithin <see cref="this"/> compnent <see cref="AbsoluteDrawingArea"/>
    /// and the mouse is moving while still pressed.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseDragEventArgs> MouseDrag = (_, _) => { };

    /// <summary>
    /// Mouse dragging start.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseDragBegin = (_, _) => { };

    /// <summary>
    /// Mouse dragging ends.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseEventArgs> MouseDragEnd = (_, _) => { };

    /// <summary>
    /// Key is pressed inside <see cref="this"/> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs> KeyPress = (_, _) => { };

    /// <summary>
    /// Key is released inside <see langword="this"/> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs> KeyRelease = (_, _) => { };

    /// <summary>
    /// Text is inserted in <see langword="this"/> <see cref="UIComponent"/>.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, TextInputEventArgs> TextInput = (_, _) => { };

    /// <summary>
    /// Mouse wheel has been moved.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, MouseWheelEventArgs> MouseWheel = (_, _) => { };

    /// <summary>
    /// Triggered when <see langword="this" /> <see cref="UIComponent"/> drawing area changes.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingAreaEventArgs> RenderingAreaChange = (_, _) => { };

    /// <summary>
    /// A frame need to be rendered. Use this event to render at the bottom of the children.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingEventArgs> RenderFrame = (_, _) => { };

    /// <summary>
    /// The children components have been rendered. Use this event handler to render on top of the children.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, RenderingEventArgs> ChildrenRendered = (_, _) => { };

    /// <summary>
    /// The application in which <see langword="this"/> <see cref="UIComponent"/> runs.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The parent <see cref="UIComponent"/> containing <see langword="this" /> <see cref="UIComponent"/>.
    /// </summary>
    public UIComponent? Parent { get; private set; }

    /// <summary>
    /// Gets the <see cref="UIRoot"/> that contain <see langword="this" /> <see cref="UIComponent"/>.
    /// If <see langword="this" /> <see cref="UIComponent"/> has not been attached to a <see cref="UIRoot"/>,
    /// the value is <see langword="null" />.
    /// </summary>
    public UIRoot? Root => Parent?.Root ?? this as UIRoot;

    /// <summary>
    /// The component unique identifier.
    /// </summary>
    public UIProperty<UIComponent, string> ID { get; }

    /// <summary>
    /// The component top-left corner X-coordinate in pixels.
    /// </summary>
    /// <remarks>The X-coordinate is relative to the parent component rendering area.</remarks>
    public UIProperty<UIComponent, PixelUnit> X { get; }

    /// <summary>
    /// The component top-left corner Y-coordinate in pixels.
    /// </summary>
    /// <remarks>The Y-coordinate is relative to the parent component rendering area.</remarks>
    public UIProperty<UIComponent, PixelUnit> Y { get; }

    /// <summary>
    /// The component width in pixels.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Width { get; }

    /// <summary>
    /// The component height in pixels.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Height { get; }

    /// <summary>
    /// Whether the component is rendered or not.
    /// </summary>
    public UIProperty<UIComponent, ComponentVisibility> Visibility { get; }

    /// <summary>
    /// Specifies how to automatically specify this component width.
    /// </summary>
    public UIProperty<UIComponent, IAutoSize> AutoWidth { get; }

    /// <summary>
    /// Specifies how to automatically specify this component height.
    /// </summary>
    public UIProperty<UIComponent, IAutoSize> AutoHeight { get; }

    /// <summary>
    /// Specifies the horizontal alignment of this component relative to its parent.
    /// </summary>
    public UIProperty<UIComponent, IHorizontalAlignment> HorizontalAlignment { get; }

    /// <summary>
    /// Specifies the vertical alignment of this component relative to its parent.
    /// </summary>
    public UIProperty<UIComponent, IVerticalAlignment> VerticalAlignment { get; }

    /// <summary>
    /// Whether this component can get focus.
    /// </summary>
    public UIProperty<UIComponent, bool> Focusable { get; }

    /// <summary>
    /// Whether this component has focus.
    /// </summary>
    public UIProperty<UIComponent, bool> Focus { get; }

    /// <summary>
    /// Whether this component is enabled and can receive events.
    /// </summary>
    public UIProperty<UIComponent, bool> Enabled { get; }

    /// <summary>
    /// The component background color.
    /// </summary>
    /// <remarks>
    /// It is the derived class responsibility to decide how to handle the background color.
    /// </remarks>
    public UIProperty<UIComponent, Color> BackgroundColor { get; }

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

        ID = new UIProperty<UIComponent, string>(this, string.Empty);
        X = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Y = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Width = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Height = new UIProperty<UIComponent, PixelUnit>(this, PixelUnit.Auto);
        Visibility = new UIProperty<UIComponent, ComponentVisibility>(this, visibility);
        AutoWidth = new UIProperty<UIComponent, IAutoSize>(this, autoWidth ?? AutoSize.Stretch);
        AutoHeight = new UIProperty<UIComponent, IAutoSize>(this, autoHeight ?? AutoSize.Stretch);
        HorizontalAlignment = new UIProperty<UIComponent, IHorizontalAlignment>(this, horizontalAlignment ?? Alignment.Center);
        VerticalAlignment = new UIProperty<UIComponent, IVerticalAlignment>(this, verticalAlignment ?? Alignment.Center);
        Focusable = new UIProperty<UIComponent, bool>(this, isFocusable);
        Focus = new UIProperty<UIComponent, bool>(this, false);
        Enabled = new UIProperty<UIComponent, bool>(this, true);
        BackgroundColor = new UIProperty<UIComponent, Color>(this, Color.Transparent);

        _wrapSize = Size.Zero;
        _relativeDrawingArea = Area.Empty;
        _absoluteDrawingArea = Area.Empty;
        _clipArea = Area.Empty;

        Focus.ValueChange += Focus_ValueChange;
        Enabled.ValueChange += Enabled_ValueChange;
        MousePress += UIComponent_MousePress;
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
        Application.EventSystem.InvalidateRendering();
    }

    /// <summary>
    /// Marks <see langword="this" /> <see cref="UIComponent"/> as no longer invalidated.
    /// It is usually not necessary to cancel invalidation because it is done automatically by the framework,
    /// but it is possible to manually override the behaviod if needed.
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
    /// when setting values that may temporarely lead to object inconsistency.
    /// However, since this method is public, validation can occurr at any time.
    /// </remarks>
    public virtual void Validate()
    {
        if (!Width.Value.IsAuto && Width.Value < 0.0f) throw new UIPropertyValidationException($"Width must be greater or equal to zero, found {Width.Value}", this);
        if (!Height.Value.IsAuto && Height.Value < 0.0f) throw new UIPropertyValidationException($"Height must be greater or equal to zero, found {Height.Value}", this);
        if (!Focusable.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not focusable", this);
        if (!Enabled.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not enabled");
    }

    /// <summary>
    /// Compute the minimum size necessary to dipslay all the component correctly.
    /// This does not include padding.
    /// </summary>
    /// <param name="childrenSize">
    /// The best estimated size to render the children correctly.
    /// This parameter can be used for layout re-computation, where the best size is the size that fits
    /// all estimate sizes of the children.
    /// </param>
    /// <returns>The minimum size necessary to dipslay all the component correctly.</returns>
    /// <remarks>
    /// Getting the minimum optimzal size for a component might not be always easy. In that case, it should
    /// at least get close to it or be as small as possible.
    /// </remarks>
    protected abstract Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize);

    /// <summary>
    /// Override this method to estabilsh custom final rendering area for <see langword="this" /> component children.
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

    /// <summary>
    /// The size of <see langword="this" /> component at the latest frame rendering.
    /// </summary>
    /// TODO: remove
    public Size ActualSize => _relativeDrawingArea.Size;

    /// <summary>
    /// Adds a child to <see langword="this" /> component.
    /// </summary>
    /// <param name="component">The child component to add.</param>
    /// <param name="index">
    /// The index where to insert the <paramref name="component"/>. If <see langword="null" /> the <paramref name="component"/> is
    /// appended to the children list.</param>
    /// <exception cref="ArgumentException">
    /// If a child with the same <see cref="ID"/> as the given <paramref name="component"/> already exists.
    /// </exception>
    protected virtual void AddChildNode(UIWidget component, int? index = null)
    {
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        component.Parent?.RemoveChildNode(component);
        component.Parent = this;
        component.AttachEventsFromParent();
        component.RecomputeLevel();
        component.InvalidateAll();
        Invalidate();
        Application.EventSystem.InvalidateRendering();
        if (index == null) _childNodes.Add(component);
        else if (index + 1 < _childNodes.Count) _childNodes.Insert((int)index + 1, component);
        else _childNodes.Add(component);
    }

    /// <summary>
    /// Gets all the child components of <see cref="this"/> comoponet.
    /// </summary>
    /// <returns>The list of child component.</returns>
    protected virtual IEnumerable<UIWidget> GetChildrenNodes() =>
        new List<UIWidget>(_childNodes);

    /// <summary>
    /// Removes the given child <paramref name="component"/> from <see cref="this"/> component.
    /// </summary>
    /// <param name="component">The child component to remove.</param>
    /// <returns><see langword="true" /> if successfully removed, otherwise <see langword="false" />.</returns>
    protected bool RemoveChildNode(UIWidget component)
    {
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        Invalidate();
        component.DetachEventsFromParent();
        if (component.Parent == this) component.Parent = null;
        return _childNodes.Remove(component);
    }

    protected void OnMousePress(MouseEventArgs e)
    {
        MousePress.Invoke(this, e);
    }

    protected void OnMouseRelease(MouseEventArgs e)
    {
        MouseRelease.Invoke(this, e);
    }

    protected void OnMouseMove(MouseEventArgs e)
    {
        MouseMove.Invoke(this, e);
    }

    protected void OnKeyPress(KeyEventArgs e)
    {
        KeyPress.Invoke(this, e);
    }

    protected void OnKeyRelease(KeyEventArgs e)
    {
        KeyRelease.Invoke(this, e);
    }

    protected void OnTextInput(TextInputEventArgs e)
    {
        TextInput.Invoke(this, e);
    }

    protected void OnMouseWheel(MouseWheelEventArgs e)
    {
        MouseWheel.Invoke(this, e);
    }

    internal IEnumerable<UIComponent> GetComponentTreeNodesDepthFirstSearch() =>
        _childNodes.Union(_childNodes.SelectMany(c => c.GetComponentTreeNodesDepthFirstSearch()));

    internal void OnRenderFrame(RenderingEventArgs renderingArgs)
    {
        Application.LifeCycle.ThrowIfNotOnRenderingPhase();

        if (Visibility.Value == ComponentVisibility.Visible)
        {
            renderingArgs.Canvas.ContainerAbsoluteDrawingArea = _absoluteDrawingArea;
            renderingArgs.Canvas.ClippingArea = _clipArea;
            RenderFrame.Invoke(this, renderingArgs);
            _childNodes.ForEach(c => c.OnRenderFrame(renderingArgs));
            renderingArgs.Canvas.ContainerAbsoluteDrawingArea = _absoluteDrawingArea;
            renderingArgs.Canvas.ClippingArea = _clipArea;
            ChildrenRendered.Invoke(this, renderingArgs);
        }
    }

    /// <summary>
    /// Computes the size of the component if <see cref="AutoSize.Wrap"/> is chose for both width and hight.
    /// </summary>
    /// <returns>
    /// <see langword="true" /> if the size has changeed since the last time thos method was called, otherwise <see langword="false" />.
    /// </returns>
    internal bool ReComputeWrapSize()
    {
        var childrenSize = _childNodes.Select(c => c._wrapSize);
        var minimalOptimalSize = ComputeMinimumOptimalSize(childrenSize);
        var width = Width.Value.IsAuto ? minimalOptimalSize.Width : Width.Value;
        var height = Height.Value.IsAuto ? minimalOptimalSize.Height : Height.Value;
        var collapsed = Visibility.Value == ComponentVisibility.Collapsed;
        var currentWrapSize = _wrapSize;
        var newWrapSize = collapsed ? Size.Zero : new Size(width, height);
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
        if (!rootCall) _relativeDrawingAreaOverride = relativeDrawingArea;
        _relativeDrawingArea = ComputeRelativeDrawingArea(_relativeDrawingAreaOverride);
        _absoluteDrawingArea = ComputeAbsoluteDrawingArea();
        _clipArea = ComputeClipArea();

        var childrenAreas = RepositionChildren(_relativeDrawingArea.Size, _childNodes.Select(c => c._wrapSize));

        if (childrenAreas.Count != 0 && childrenAreas.Count != _childNodes.Count)
        {
            throw new InvalidOperationException($"{nameof(RepositionChildren)} must return the same number of elements as the number of children or be empty: {childrenAreas.Count()} provided but {_childNodes.Count} exist");
        }

        // TODO: no need to go recursively if nothing has changed
        for (var i = 0; i < _childNodes.Count; i++)
        {
            var child = _childNodes[i];
            // The parent is already invalidated, so no need to invalidate this component. This ensures that if a component is invalidated already, the children won't
            // TODO: if using rendering instancing, make sure to invalidate each component that has changed.
            // then we will go through each and update the instance buffer portion accordingly.
            // As opposed to now, this will ensure that each invalidated component is in the invalidated list so it is possible to update instances.
            child.CancelInvalidation();
            var childArea = childrenAreas.Count != 0 ? childrenAreas[i] : null;
            child.ComputeDrawingAreas(childArea);
        }

        // TODO: Add PostRepositionChildren(relativeArea, childrenDrawingAreaList) to allow re-repositioning children after knowing their size.
        // This second pass layout is very useful for scoll view, so you can re-implement it better.
        // The implementation would be
        // - Make sure X and Y are not negative but (0, 0) if the element fits the scroll view (it happens when treebox click on unfold button and the size of the box reduces so much that scroll bars disappear)
        // - Make scroll bars as rectangles and decide their size based on child size.
        // And finally remove ActualSize property which is dangerous.
        // Also make sure all drawing areas of children are re calculated recursively in the sub tree.
        Validate();
        RenderingAreaChange.Invoke(this, new RenderingAreaEventArgs(_relativeDrawingArea));
    }

    private void UIComponent_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            MouseDragBegin.Invoke(this, e);
            _mouseDragPointAbsolute = e.AbsoluteLocation;
            _mouseLastDragPointAbsolute = e.AbsoluteLocation;
        }
    }

    private void AttachEventsFromParent()
    {
        if (Parent == null) return;
        Parent.MousePress += _parent_MousePress;
        Parent.MouseRelease += _parent_MouseRelease;
        Parent.MouseMove += _parent_MouseMove;
        Parent.KeyPress += _parent_KeyPress;
        Parent.KeyRelease += _parent_KeyRelease;
        Parent.TextInput += _parent_TextInput;
        Parent.MouseWheel += _parent_MouseWheel;
    }

    private void DetachEventsFromParent()
    {
        if (Parent == null) return;
        Parent.MousePress -= _parent_MousePress;
        Parent.MouseRelease -= _parent_MouseRelease;
        Parent.MouseMove -= _parent_MouseMove;
        Parent.KeyPress -= _parent_KeyPress;
        Parent.KeyRelease -= _parent_KeyRelease;
        Parent.TextInput -= _parent_TextInput;
        Parent.MouseWheel -= _parent_MouseWheel;
    }

    private void _parent_MouseMove(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        _mouseAbsoluteLocation = mouseEventArgs.AbsoluteLocation;
        if (mouseEventArgs.AbsoluteLocation.IsWithin(_absoluteDrawingArea) && Visibility.Value == ComponentVisibility.Visible && Enabled.Value)
        {
            MouseMove.Invoke(this, new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                                      mouseEventArgs.AbsoluteLocation - _absoluteDrawingArea.TopLeft,
                                                      mouseEventArgs.Button));
        }

        if (_mouseDragPointAbsolute != null && _mouseLastDragPointAbsolute != null)
        {
            var offset = mouseEventArgs.AbsoluteLocation - _mouseLastDragPointAbsolute;
            _mouseLastDragPointAbsolute = mouseEventArgs.AbsoluteLocation;
            MouseDrag.Invoke(this, new MouseDragEventArgs(_mouseDragPointAbsolute, _mouseLastDragPointAbsolute, offset));
        }
    }

    private void _parent_MouseRelease(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        var e = new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                   mouseEventArgs.AbsoluteLocation - _absoluteDrawingArea.TopLeft,
                                   mouseEventArgs.Button);

        if (mouseEventArgs.AbsoluteLocation.IsWithin(_absoluteDrawingArea) && Visibility.Value == ComponentVisibility.Visible && Enabled.Value)
        {
            MouseRelease.Invoke(this, e);
        }

        if (_mouseDragPointAbsolute != null)
        {
            MouseDragEnd.Invoke(this, e);
        }

        _mouseDragPointAbsolute = null;
        _mouseLastDragPointAbsolute = null;
    }

    private void _parent_MousePress(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.AbsoluteLocation.IsWithin(_absoluteDrawingArea) && Visibility.Value == ComponentVisibility.Visible && Enabled.Value)
        {
            MousePress.Invoke(this, new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                                       mouseEventArgs.AbsoluteLocation - _absoluteDrawingArea.TopLeft,
                                                       mouseEventArgs.Button));
        }
    }

    private void _parent_KeyPress(UIComponent sender, KeyEventArgs keyEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus.Value || !Focusable.Value))
        {
            KeyPress.Invoke(this, keyEventArgs);
        }
    }

    private void _parent_KeyRelease(UIComponent sender, KeyEventArgs keyEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus.Value || this is UIContainer))
        {
            KeyRelease.Invoke(this, keyEventArgs);
        }
    }

    private void _parent_TextInput(UIComponent sender, TextInputEventArgs textInputEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus.Value || this is UIContainer))
        {
            TextInput.Invoke(this, textInputEventArgs);
        }
    }

    private void _parent_MouseWheel(UIComponent sender, MouseWheelEventArgs mouseWheelEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && _mouseAbsoluteLocation != null && _mouseAbsoluteLocation.IsWithin(_absoluteDrawingArea))
        {
            MouseWheel.Invoke(this, mouseWheelEventArgs);
        }
    }

    private void Focus_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        if (e.CurrentValue)
        {
            RequestFocusFor(this);
        }
    }

    private void Enabled_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        if (!e.CurrentValue)
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

    private Area ComputeRelativeDrawingArea(Area? areaOverride = null)
    {
        if (Visibility.Value == ComponentVisibility.Collapsed) return Area.Empty;
        var sizeOverride = areaOverride?.Size ?? new Size(PixelUnit.Auto, PixelUnit.Auto);
        var locationOverride = areaOverride?.TopLeft ?? new Point(PixelUnit.Auto, PixelUnit.Auto);
        var parentSize = Parent?._relativeDrawingArea?.Size ?? Application.ScreenSize;

        var autoWidth = AutoWidth.Value.ComputeWidth(parentSize, _wrapSize);
        var autoHeight = AutoHeight.Value.ComputeHeight(parentSize, _wrapSize);
        var actualWidth = sizeOverride.Width.IsAuto ? Width.Value.IsAuto ? autoWidth : Width.Value : sizeOverride.Width;
        var actualHeight = sizeOverride.Height.IsAuto ? Height.Value.IsAuto ? autoHeight : Height.Value : sizeOverride.Height;
        var actualSize = new Size(actualWidth, actualHeight);

        var autoX = HorizontalAlignment.Value.ComputeX(parentSize, actualSize);
        var autoY = VerticalAlignment.Value.ComputeY(parentSize, actualSize);
        var actualX = locationOverride.X.IsAuto ? X.Value.IsAuto ? autoX : X.Value : locationOverride.X;
        var actualY = locationOverride.Y.IsAuto ? Y.Value.IsAuto ? autoY : Y.Value : locationOverride.Y;
        var actualTopLeft = new Point(actualX, actualY);

        // Windows X and Y position will be relative to the screen, but the relative area location is Point.Zero, because it is relative to the viewport (i.e. the window itslef).
        return new Area(actualTopLeft, actualSize);
    }

    private Area ComputeAbsoluteDrawingArea() => Parent != null ? _relativeDrawingArea.ToAbsolute(Parent._absoluteDrawingArea) : _relativeDrawingArea.Fill();
    private Area ComputeClipArea() => _absoluteDrawingArea.Clip(Parent?._clipArea);

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

        _childNodes.ForEach(c => c.RecomputeLevel());
    }

    private void InvalidateAll()
    {
        if (Root == null) return;
        Invalidate();
        _childNodes.ForEach(c => c.InvalidateAll());
    }

    private void CancelInvalidationAll()
    {
        if (Root == null) return;
        CancelInvalidation();
        _childNodes.ForEach(c => c.CancelInvalidation());
    }
}
