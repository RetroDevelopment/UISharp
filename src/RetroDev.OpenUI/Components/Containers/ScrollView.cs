using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Graphics.Shapes;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.UI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// Allows to scroll the child component in case it is bigger than the scroll view size.
/// </summary>
public class ScrollView : UIContainer, ISingleContainer
{
    private const int ScrollBarSize = 15;
    private UIWidget? _child;
    private bool _moveHorizontalBar = false;
    private bool _moveVerticalBar = false;

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.Any() ? childrenSize.First() : Size.Zero;

    /// <inheritdoc />
    public override IEnumerable<UIWidget> Children => GetChildrenNodes();

    /// <summary>
    /// The color of the horizontal and vertical scroll bars.
    /// </summary>
    public UIProperty<ScrollView, Color> ScrollBarColor { get; }

    // TODO: scroll interval in pixels (hor and vert)
    // TODO: scroll arrows
    // TODO: scroll with mouse wheel

    /// <summary>
    /// Creates a new scroll view.
    /// </summary>
    /// <param name="parent">The application that contain this scroll view.</param>
    public ScrollView(Application parent) : base(parent)
    {
        ScrollBarColor = new UIProperty<ScrollView, Color>(this, Application.Theme.PrimaryColorContrast, bindingType: BindingType.DestinationToSource);

        ChildrenRendered += ScrollView_ChildrenRendered;
        MouseDrag += ScrollView_MouseDrag;
        MouseDragBegin += ScrollView_MouseDragBegin;
        MouseDragEnd += ScrollView_MouseDragEnd;
        MouseWheel += ScrollView_MouseWheel;
    }

    /// <summary>
    /// Sets the component to be inserted in <see langword="this" /> scroll view.
    /// </summary>
    /// <param name="component">The component to be inserted in <see langword="this" /> scroll view.</param>
    public void SetComponent(UIWidget component)
    {
        if (_child != null) RemoveChildNode(_child);
        _child = component;

        if (_child.X.Value == PixelUnit.Auto) _child.X.Value = 0;
        if (_child.Y.Value == PixelUnit.Auto) _child.Y.Value = 0;

        AddChildNode(_child);
    }

    private void ScrollView_ChildrenRendered(UIComponent sender, RenderingEventArgs e)
    {
        var canvas = e.Canvas;
        var horizontalScrollBarArea = GetHorizontalScrollBarArea();
        var verticalScrollBarArea = GetVerticalScrollBarArea();

        if (horizontalScrollBarArea != null)
        {
            var radius = Math.Min(Math.Min(horizontalScrollBarArea.Size.Width / 2.0f, horizontalScrollBarArea.Size.Height / 2.0f), ScrollBarSize / 2.0f);
            var scrollBarShape = new Rectangle(BackgroundColor: ScrollBarColor.Value.WithAlpha(100),
                                               CornerRadiusX: radius,
                                               CornerRadiusY: radius);
            canvas.Render(scrollBarShape, horizontalScrollBarArea);
        }

        if (verticalScrollBarArea != null)
        {
            var radius = Math.Min(Math.Min(verticalScrollBarArea.Size.Width / 2.0f, verticalScrollBarArea.Size.Height / 2.0f), ScrollBarSize / 2.0f);
            var scrollBarShape = new Rectangle(BackgroundColor: ScrollBarColor.Value.WithAlpha(100),
                                               CornerRadiusX: radius,
                                               CornerRadiusY: radius);
            canvas.Render(scrollBarShape, verticalScrollBarArea);
        }
    }

    private void ScrollView_MouseDragBegin(UIComponent sender, MouseEventArgs e)
    {
        var horizontalScrollBarArea = GetHorizontalScrollBarArea();
        var verticalScrollBarArea = GetVerticalScrollBarArea();

        if (horizontalScrollBarArea != null && e.RelativeLocation.IsWithin(horizontalScrollBarArea))
        {
            _moveHorizontalBar = true;
        }

        if (verticalScrollBarArea != null && e.RelativeLocation.IsWithin(verticalScrollBarArea))
        {
            _moveVerticalBar = true;
        }
    }

    private void ScrollView_MouseDragEnd(UIComponent sender, MouseEventArgs e)
    {
        _moveHorizontalBar = false;
        _moveVerticalBar = false;
    }

    private void ScrollView_MouseWheel(UIComponent sender, MouseWheelEventArgs e)
    {
        // TODO: mouseDrag event and this event join in same method
        if (_child == null) return;
        var mouseWheelSpeed = 4.0f; // How fast scroll should go.

        var horizontalScrollBarArea = GetHorizontalScrollBarArea();
        var verticalScrollBarArea = GetVerticalScrollBarArea();
        var maximumHorizontalScrollBarAreaWidth = GetMaximumHorizontalScrollBarAreaWidth();
        var maximumVerticalScrollBarAreaHeight = GetMaximumVerticalScrollBarAreaHeight();

        if (!_moveHorizontalBar && horizontalScrollBarArea != null)
        {
            var offsetXFactor = -(e.HorizontalMovement * mouseWheelSpeed) / (maximumHorizontalScrollBarAreaWidth - horizontalScrollBarArea.Size.Width);
            var maximumChildHorizontalScroll = GetMaximumChildHorizontalScroll();
            _child.X.Value = Math.Clamp(_child.X.Value - offsetXFactor * maximumChildHorizontalScroll, -maximumChildHorizontalScroll, 0.0f);
        }

        if (!_moveVerticalBar && verticalScrollBarArea != null)
        {
            var offsetYFactor = -(e.VerticalMovement * mouseWheelSpeed) / (maximumVerticalScrollBarAreaHeight - verticalScrollBarArea.Size.Height);
            var maximumChildVerticalScroll = GetMaximumChildVerticalScroll();
            _child.Y.Value = Math.Clamp(_child.Y.Value - offsetYFactor * maximumChildVerticalScroll, -maximumChildVerticalScroll, 0.0f);
        }

    }

    private void ScrollView_MouseDrag(UIComponent sender, MouseDragEventArgs e)
    {
        if (_child == null) return;

        var horizontalScrollBarArea = GetHorizontalScrollBarArea();
        var verticalScrollBarArea = GetVerticalScrollBarArea();
        var maximumHorizontalScrollBarAreaWidth = GetMaximumHorizontalScrollBarAreaWidth();
        var maximumVerticalScrollBarAreaHeight = GetMaximumVerticalScrollBarAreaHeight();

        if (_moveHorizontalBar && horizontalScrollBarArea != null)
        {
            var offsetXFactor = e.Offset.X / (maximumHorizontalScrollBarAreaWidth - horizontalScrollBarArea.Size.Width);
            var maximumChildHorizontalScroll = GetMaximumChildHorizontalScroll();
            _child.X.Value = Math.Clamp(_child.X.Value - offsetXFactor * maximumChildHorizontalScroll, -maximumChildHorizontalScroll, 0.0f);
        }

        if (_moveVerticalBar && verticalScrollBarArea != null)
        {
            var offsetYFactor = e.Offset.Y / (maximumVerticalScrollBarAreaHeight - verticalScrollBarArea.Size.Height);
            var maximumChildVerticalScroll = GetMaximumChildVerticalScroll();
            _child.Y.Value = Math.Clamp(_child.Y.Value - offsetYFactor * maximumChildVerticalScroll, -maximumChildVerticalScroll, 0.0f);
        }
    }

    private Area? GetHorizontalScrollBarArea()
    {
        if (_child == null) return null;

        var scrollViewSize = ActualSize;
        var childSize = _child.ActualSize;
        var maximumHorizontalScrollBarAreaWidth = GetMaximumHorizontalScrollBarAreaWidth();
        var widthFactor = maximumHorizontalScrollBarAreaWidth / childSize.Width;

        if (widthFactor < 1.0f)
        {
            var maximumChildHorizontalScroll = GetMaximumChildHorizontalScroll();
            var childHorizontalScroll = Math.Clamp(-_child.X.Value, 0.0f, maximumChildHorizontalScroll);
            var childHorizontalScrollFactor = childHorizontalScroll / maximumChildHorizontalScroll;
            var horizontalScollBarSize = new Size(maximumHorizontalScrollBarAreaWidth * widthFactor, ScrollBarSize);
            var maximumHorizontalBarX = maximumHorizontalScrollBarAreaWidth - horizontalScollBarSize.Width;
            var horizontalScrollBarLocation = new Point(maximumHorizontalBarX * childHorizontalScrollFactor,
                                                        scrollViewSize.Height - ScrollBarSize);
            return new Area(horizontalScrollBarLocation, horizontalScollBarSize);
        }

        return null;
    }

    private Area? GetVerticalScrollBarArea()
    {
        if (_child == null) return null;

        var scrollViewSize = ActualSize;
        var childSize = _child.ActualSize;
        var maximumVerticalScollBarAreaHeight = GetMaximumVerticalScrollBarAreaHeight();
        var heightFactor = maximumVerticalScollBarAreaHeight / childSize.Height;

        if (heightFactor < 1.0f)
        {
            var maximumChildVerticalScroll = GetMaximumChildVerticalScroll();
            var childVerticalScroll = Math.Clamp(-_child.Y.Value, 0.0f, maximumChildVerticalScroll);
            var childVerticalScrollFactor = childVerticalScroll / maximumChildVerticalScroll;
            var verticalScollBarSize = new Size(ScrollBarSize, maximumVerticalScollBarAreaHeight * heightFactor);
            var maximumVerticalBarY = maximumVerticalScollBarAreaHeight - verticalScollBarSize.Height;
            var verticalScrollBarLocation = new Point(scrollViewSize.Width - ScrollBarSize,
                                                maximumVerticalBarY * childVerticalScrollFactor);
            return new Area(verticalScrollBarLocation, verticalScollBarSize);
        }

        return null;
    }

    private PixelUnit GetMaximumChildHorizontalScroll() => _child!.ActualSize.Width - ActualSize.Width;
    private PixelUnit GetMaximumChildVerticalScroll() => _child!.ActualSize.Height - ActualSize.Height;
    private PixelUnit GetMaximumHorizontalScrollBarAreaWidth() => ActualSize.Width;
    private PixelUnit GetMaximumVerticalScrollBarAreaHeight() => ActualSize.Height;
}
