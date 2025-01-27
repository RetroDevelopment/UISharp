using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Components.Core;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.Utils;
using SixLabors.ImageSharp.Formats.Bmp;

namespace RetroDev.OpenUI.Components;

// TODO: Same way as before, add a TextResourceManager class to manage text (e.g. with language) and create it in Application.
// They can also have dictionary/xml. Then bind these properties in your project and you will get automatic language change.

/// <summary>
/// The abstract calss for all UI components (windows, buttons, etc.).
/// </summary>
public abstract class UIComponent
{
    internal readonly List<UIComponent> _children = [];
    private UIComponent? _focusedComponent;
    private Point? _mouseDragPointAbsolute = null;
    private Point? _mouseLastDragPointAbsolute = null;

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
    /// Key is pressed inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs> KeyPress = (_, _) => { };

    /// <summary>
    /// Key is released inside <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, KeyEventArgs> KeyRelease = (_, _) => { };

    /// <summary>
    /// Text is inserted in <see cref="this"/> window.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, TextInputEventArgs> TextInput = (_, _) => { };

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
    /// Gets the root component, usually a <see cref="Window"/>.
    /// </summary>
    public UIComponent Root => Parent?.Root ?? this;

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
    /// The size (in pixels) of the container in which <see langword="this" /> <see cref="UIComponent"/> is diplayed.
    /// If <see langword="this" /> <see cref="UIComponent"/> is <see cref="Root"/> (e.g. a <see cref="Window"/>, the
    /// container is assumed to be the main screen, so the main screen resolution will be returned.
    /// </summary>
    public Size ContainerSize => Parent?.RelativeDrawingArea?.Size ?? Application.ScreenSize;

    /// <summary>
    /// The ideal component size which allows to correctly display the whole component.
    /// </summary>  
    internal Size SizeHint { get; private set; }


    /// <summary>
    /// The 2D area (in pixels) where this component is rendered. The area is relative to the parent area,
    /// so [(0, 0), (100, 100)] would indicate that the component is rendered at the top-left of the paraent component,
    /// and it has size 100x100 pixels.
    /// </summary>
    internal Area RelativeDrawingArea { get; private set; }

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
    internal Area AbsoluteDrawingArea { get; private set; }

    /// <summary>
    /// The area in which where the UI drawing occurs. Every pixel outside of this area will be clipped.
    /// </summary>
    internal Area ClipArea { get; private set; }

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

        SizeHint = Size.Zero;
        RelativeDrawingArea = Area.Empty;
        AbsoluteDrawingArea = Area.Empty;
        ClipArea = Area.Empty;

        Focus.ValueChange += Focus_ValueChange;
        Enabled.ValueChange += Enabled_ValueChange;
        MousePress += UIComponent_MousePress;
    }

    // TODO: group protected and publics together

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
    protected abstract Size ComputeSizeHint(IEnumerable<Size> childrenSize);

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

    public Size ComputeSizeHint()
    {
        var childrenSize = _children.Select(c => c.ComputeSizeHint()).ToList(); // TODO: only if invalidated call ComputeSizeHint() otherise use SizeHint
        var sizeHint = ComputeSizeHint(childrenSize);
        var width = Width.Value.IsAuto ? sizeHint.Width : Width.Value;
        var height = Height.Value.IsAuto ? sizeHint.Height : Height.Value;
        SizeHint = new Size(width, height);
        return SizeHint;
    }

    public void ComputeDrawingAreas(Area? relativeDrawingArea = null)
    {
        if (relativeDrawingArea != null)
        {
            RelativeDrawingArea = ComputeRelativeDrawingArea(relativeDrawingArea);
        }
        else
        {
            RelativeDrawingArea = ComputeRelativeDrawingArea();
        }

        AbsoluteDrawingArea = ComputeAbsoluteDrawingArea();
        ClipArea = ComputeClipArea();

        var childrenAreas = RepositionChildren(RelativeDrawingArea.Size, _children.Select(c => c.SizeHint));

        if (childrenAreas.Count != 0 && childrenAreas.Count != _children.Count)
        {
            throw new InvalidOperationException($"{nameof(RepositionChildren)} must return the same number of elements as the number of children or be empty: {childrenAreas.Count()} provided but {_children.Count} exist");
        }

        for (var i = 0; i < _children.Count; i++)
        {
            var child = _children[i];
            var childArea = childrenAreas.Count != 0 ? childrenAreas[i] : null;
            child.ComputeDrawingAreas(childArea);
        }

        Validate();
    }

    public virtual void Validate()
    {
        if (!Width.Value.IsAuto && Width.Value < 0.0f) throw new UIPropertyValidationException($"Width must be greater or equal to zero, found {Width.Value}", this);
        if (!Height.Value.IsAuto && Height.Value < 0.0f) throw new UIPropertyValidationException($"Height must be greater or equal to zero, found {Height.Value}", this);
        if (!Focusable.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not focusable", this);
        if (!Enabled.Value && Focus.Value) throw new UIPropertyValidationException("Cannot focus a component that is not enabled");
    }


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
    protected virtual void AddChild(UIComponent component, int? index = null)
    {
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        component.Parent?.RemoveChild(component);
        component.Parent = this;
        component.AttachEventsFromParent();
        Application._eventSystem.InvalidateRendering();
        if (index == null) _children.Add(component);
        else if (index + 1 < _children.Count) _children.Insert((int)index + 1, component);
        else _children.Add(component);
    }

    /// <summary>
    /// Gets all the child components of <see cref="this"/> comoponet.
    /// </summary>
    /// <returns>The list of child component.</returns>
    protected virtual IEnumerable<UIComponent> GetChildren() =>
        new List<UIComponent>(_children);

    /// <summary>
    /// Removes the given child <paramref name="component"/> from <see cref="this"/> component.
    /// </summary>
    /// <param name="component">The child component to remove.</param>
    /// <returns><see langword="true" /> if successfully removed, otherwise <see langword="false" />.</returns>
    protected bool RemoveChild(UIComponent component)
    {
        Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        component.DetachEventsFromParent();
        if (component.Parent == this) component.Parent = null;
        return _children.Remove(component);
    }

    protected void OnMousePress(MouseEventArgs e)
    {
        MousePress?.Invoke(this, e);
    }

    protected void OnMouseRelease(MouseEventArgs e)
    {
        MouseRelease?.Invoke(this, e);
    }

    protected void OnMouseMove(MouseEventArgs e)
    {
        MouseMove?.Invoke(this, e);
    }

    protected void OnKeyPress(KeyEventArgs e)
    {
        KeyPress?.Invoke(this, e);
    }

    protected void OnKeyRelease(KeyEventArgs e)
    {
        KeyRelease?.Invoke(this, e);
    }

    protected void OnTextInput(TextInputEventArgs e)
    {
        TextInput?.Invoke(this, e);
    }

    internal IEnumerable<UIComponent> GetComponentTreeNodesDepthFirstSearch() =>
        _children.Union(_children.SelectMany(c => c.GetComponentTreeNodesDepthFirstSearch()));

    internal void OnRenderFrame(RenderingEventArgs renderingArgs)
    {
        Application.LifeCycle.ThrowIfNotOnRenderingPhase();

        if (Visibility.Value == ComponentVisibility.Visible)
        {
            renderingArgs.Canvas.ContainerAbsoluteDrawingArea = AbsoluteDrawingArea;
            renderingArgs.Canvas.ClippingArea = ClipArea;
            RenderFrame.Invoke(this, renderingArgs);
            _children.ForEach(c => c.OnRenderFrame(renderingArgs));
            renderingArgs.Canvas.ContainerAbsoluteDrawingArea = AbsoluteDrawingArea;
            renderingArgs.Canvas.ClippingArea = ClipArea;
            ChildrenRendered.Invoke(this, renderingArgs);
        }
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
    }

    private void _parent_MouseMove(UIComponent sender, MouseEventArgs mouseEventArgs)
    {
        if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea) && Visibility.Value == Components.ComponentVisibility.Visible && Enabled)
        {
            MouseMove.Invoke(this, new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                                      mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
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
                                   mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                   mouseEventArgs.Button);

        if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea) && Visibility.Value == Components.ComponentVisibility.Visible && Enabled)
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
        if (mouseEventArgs.AbsoluteLocation.IsWithin(AbsoluteDrawingArea) && Visibility.Value == Components.ComponentVisibility.Visible && Enabled)
        {
            MousePress.Invoke(this, new MouseEventArgs(mouseEventArgs.AbsoluteLocation,
                                                       mouseEventArgs.AbsoluteLocation - AbsoluteDrawingArea.TopLeft,
                                                       mouseEventArgs.Button));
        }
    }

    private void _parent_KeyPress(UIComponent sender, KeyEventArgs keyEventArgs)
    {
        if (Visibility.Value == Components.ComponentVisibility.Visible && (Focus || !Focusable))
        {
            KeyPress.Invoke(this, new KeyEventArgs(keyEventArgs.Button));
        }
    }

    private void _parent_KeyRelease(UIComponent sender, KeyEventArgs keyEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus || this is Container))
        {
            KeyRelease.Invoke(this, new KeyEventArgs(keyEventArgs.Button));
        }
    }

    private void _parent_TextInput(UIComponent sender, TextInputEventArgs textInputEventArgs)
    {
        if (Visibility.Value == ComponentVisibility.Visible && (Focus || this is Container))
        {
            TextInput.Invoke(this, new TextInputEventArgs(textInputEventArgs.Text));
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
        // Only the root component can manage focus, because only one object can be focusable at a time in a window.
        // TODO: When implementing focus groups, just change the logic here to not delegate this to the parent.
        if (Parent != null)
        {
            Root.RequestFocusFor(component);
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

        var autoWidth = AutoWidth.Value.ComputeWidth(this);
        var autoHeight = AutoHeight.Value.ComputeHeight(this);
        var actualWidth = sizeOverride.Width.IsAuto ? (Width.Value.IsAuto ? autoWidth : Width.Value) : sizeOverride.Width;
        var actualHeight = sizeOverride.Height.IsAuto ? (Height.Value.IsAuto ? autoHeight : Height.Value) : sizeOverride.Height;
        var actualSize = new Size(actualWidth, actualHeight);

        var autoX = HorizontalAlignment.Value.ComputeX(this, actualSize);
        var autoY = VerticalAlignment.Value.ComputeY(this, actualSize);
        var actualX = locationOverride.X.IsAuto ? (X.Value.IsAuto ? autoX : X.Value) : locationOverride.X;
        var actualY = locationOverride.Y.IsAuto ? (Y.Value.IsAuto ? autoY : Y.Value) : locationOverride.Y;
        var actualTopLeft = new Point(actualX, actualY);

        return new Area(actualTopLeft, actualSize);
    }

    private Area ComputeAbsoluteDrawingArea() => RelativeDrawingArea.ToAbsolute(Parent?.AbsoluteDrawingArea);
    private Area ComputeClipArea() => AbsoluteDrawingArea.Clip(Parent?.ClipArea);
}
