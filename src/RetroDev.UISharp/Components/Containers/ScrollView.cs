using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Containers;

/// <summary>
/// Allows to scroll the child component in case it is bigger than the scroll view size.
/// </summary>
public class ScrollView : UISingleContainer
{
    private readonly ScrollBar _verticalScrollBar;
    private readonly ScrollBar _horizontalScrollBar;

    private bool _hasChild = false;
    private PixelUnit? _childVerticalPositionDragBegin;
    private PixelUnit? _childHorizontalPositionDragBegin;

    /// <summary>
    /// The color of the horizontal and vertical scroll bars.
    /// </summary>
    public UIProperty<Color> ScrollBarColor { get; }

    /// <summary>
    /// How big the scroll bars should be.
    /// </summary>
    public UIProperty<PixelUnit> ScrollBarThickness { get; }

    // TODO: scroll interval in pixels (hor and vert)

    /// <summary>
    /// Creates a new scroll view.
    /// </summary>
    /// <param name="application">The application that contain this scroll view.</param>
    public ScrollView(Application application) : base(application)
    {
        ScrollBarColor = new UIProperty<Color>(this, Color.Transparent);
        ScrollBarThickness = new UIProperty<PixelUnit>(this, 15);

        BackgroundColor.BindTheme(UISharpColorNames.ScrollViewBackground);
        ScrollBarColor.BindTheme(UISharpColorNames.ScrollViewBars, c => c.WithAlpha(100));
        BorderColor.BindTheme(UISharpColorNames.ScrollViewBorder);

        _verticalScrollBar = new ScrollBar(application);
        _verticalScrollBar.BackgroundColor.BindSourceToDestination(ScrollBarColor);
        _verticalScrollBar.Width.BindSourceToDestination(ScrollBarThickness);
        _verticalScrollBar.HorizontalAlignment.Value = Alignment.Right;
        _verticalScrollBar.MouseDragBegin += VerticalScrollBar_MouseDragBegin;
        _verticalScrollBar.MouseDrag += VerticalScrollBar_MouseDrag;
        _verticalScrollBar.MouseDragEnd += VerticalScrollBar_MouseDragEnd;
        Children.Add(_verticalScrollBar);

        _horizontalScrollBar = new ScrollBar(application);
        _horizontalScrollBar.BackgroundColor.BindSourceToDestination(ScrollBarColor);
        _horizontalScrollBar.Height.BindSourceToDestination(ScrollBarThickness);
        _horizontalScrollBar.VerticalAlignment.Value = Alignment.Bottom;
        _horizontalScrollBar.MouseDragBegin += HorizontalScrollBar_MouseDragBegin;
        _horizontalScrollBar.MouseDrag += HorizontalScrollBar_MouseDrag;
        _horizontalScrollBar.MouseDragEnd += HorizontalScrollBar_MouseDragEnd;
        Children.Add(_horizontalScrollBar);

        Item.ValueChange.Subscribe(OnChildChange);

        MouseWheel += ScrollView_MouseWheel;
        application.SecondPassMeasure += Application_SecondPassMeasure;
        application.ApplicationQuit += Application_ApplicationQuit;
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.FirstOrDefault() ?? Size.Zero;

    private void VerticalScrollBar_MouseDragBegin(UIComponent sender, MouseEventArgs e)
    {
        var child = Item.Value;
        if (child == null) return;
        _childVerticalPositionDragBegin = child.Y.Value;
    }

    private void VerticalScrollBar_MouseDrag(UIComponent sender, MouseEventArgs e)
    {
        var child = Item.Value;
        if (child == null) return;

        child.CaptureActualPosition();
        var delta = _verticalScrollBar.Delta!.Y;
        var childSizeScrollViewRatio = child.ActualSize.Height / ActualSize.Height;
        child.Y.Value = _childVerticalPositionDragBegin! - delta * childSizeScrollViewRatio;
    }

    private void VerticalScrollBar_MouseDragEnd(UIComponent sender, EventArgs e)
    {
        _childVerticalPositionDragBegin = null;
    }

    private void HorizontalScrollBar_MouseDragBegin(UIComponent sender, MouseEventArgs e)
    {
        var child = Item.Value;
        if (child == null) return;
        _childHorizontalPositionDragBegin = child.X.Value;
    }

    private void HorizontalScrollBar_MouseDrag(UIComponent sender, MouseEventArgs e)
    {
        var child = Item.Value;
        if (child == null) return;

        child.CaptureActualPosition();
        var delta = _horizontalScrollBar.Delta!.X;
        var childSizeScrollViewRatio = child.ActualSize.Width / ActualSize.Width;
        child.X.Value = _childHorizontalPositionDragBegin! - delta * childSizeScrollViewRatio;
    }

    private void HorizontalScrollBar_MouseDragEnd(UIComponent sender, EventArgs e)
    {
        _childHorizontalPositionDragBegin = null;
    }

    private void ScrollView_MouseWheel(UIComponent sender, MouseWheelEventArgs e)
    {
        var child = Item.Value;

        if (child == null) return;
        child.CaptureActualPosition();
        var horizontalMetrics = GetScrollBarMetrics(child.X.Value, ActualSize.Width, child.ActualSize.Width);
        var verticalMetrics = GetScrollBarMetrics(child.Y.Value, ActualSize.Height, child.ActualSize.Height);

        var childSize = child.ActualSize;
        // Scroll by 30% of full child size for each mouse wheel movement
        if (horizontalMetrics.ShouldDisplay) child.X.Value += (e.HorizontalMovement / 30.0f) * childSize.Width;
        if (verticalMetrics.ShouldDisplay) child.Y.Value += (e.VerticalMovement / 30.0f) * childSize.Height;
    }

    private void Application_SecondPassMeasure(Application sender, EventArgs e)
    {
        var child = Item.Value;

        if (child == null)
        {
            _horizontalScrollBar.Visibility.Value = ComponentVisibility.Hidden;
            _verticalScrollBar.Visibility.Value = ComponentVisibility.Hidden;
            return;
        }

        child.CaptureActualPosition();
        AdjustChildHorizontalPosition();
        AdjustChildVerticalPosition();
        UpdateHorizontalScrollBarSizeAndPosition();
        UpdateVerticalScrollBarSizeAndPosition();
    }

    private void Application_ApplicationQuit(Application sender, EventArgs e)
    {
        Application.SecondPassMeasure -= Application_SecondPassMeasure;
    }

    private void AdjustChildHorizontalPosition()
    {
        var child = Item.Value;
        if (child == null) return;
        child.X.Value = GetAdjustedPosition(ActualSize.Width, child.ActualSize.Width, child.X.Value);
    }

    private void AdjustChildVerticalPosition()
    {
        var child = Item.Value;
        if (child == null) return;
        child.Y.Value = GetAdjustedPosition(ActualSize.Height, child.ActualSize.Height, child.Y.Value);
    }

    private void UpdateVerticalScrollBarSizeAndPosition()
    {
        var child = Item.Value;

        if (child == null) return;

        var verticalScrollBarMetrics = GetScrollBarMetrics(child.Y.Value, ActualSize.Height, child.ActualSize.Height);
        if (verticalScrollBarMetrics.ShouldDisplay)
        {
            _verticalScrollBar.Visibility.Value = ComponentVisibility.Visible;
            _verticalScrollBar.Y.Value = verticalScrollBarMetrics.BarStart;
            _verticalScrollBar.Height.Value = verticalScrollBarMetrics.BarSize;
        }
        else
        {
            _verticalScrollBar.Visibility.Value = ComponentVisibility.Hidden;
        }
    }

    private void UpdateHorizontalScrollBarSizeAndPosition()
    {
        var child = Item.Value;

        if (child == null) return;

        var horizontalScrollBarMetrics = GetScrollBarMetrics(child.X.Value, ActualSize.Width, child.ActualSize.Width);
        if (horizontalScrollBarMetrics.ShouldDisplay)
        {
            _horizontalScrollBar.X.Value = horizontalScrollBarMetrics.BarStart;
            _horizontalScrollBar.Width.Value = horizontalScrollBarMetrics.BarSize;
            _horizontalScrollBar.Visibility.Value = ComponentVisibility.Visible;
        }
        else
        {
            _horizontalScrollBar.Visibility.Value = ComponentVisibility.Hidden;
        }
    }

    // Ensures that the child can be managed correctly inside this scroll view.
    // This method apply to one dimension, and it is applied to both X and Y coordinates. 
    private PixelUnit GetAdjustedPosition(float scrollViewSize, float childSize, float childPosition)
    {
        var maximumChildOffset = GetMaximumChildOffset(scrollViewSize, childSize);

        // If the child can fit all in the scroll view, it has to be fully visible inside the view
        // because no scroll bar will be available.
        // So fall back to auto coordinates will be performed.
        if (scrollViewSize > childSize)
        {
            return PixelUnit.Auto;
        }
        // Otherwise if the child is bigger than the scroll view and it is positioned with a positive
        // offset, it is reset to position zero, because the scroll bar can only scroll children with a negative offset.
        else if (childPosition > 0.0f)
        {
            return PixelUnit.Zero;
        }
        // Otherwise if the child is bigger than the scroll view and it is scrolled too far ahead that the scroll bar could not recover it
        // set it of the maximum negative offset.
        else if (childPosition < -maximumChildOffset)
        {
            return -maximumChildOffset;
        }
        else
        {
            return childPosition;
        }
    }

    private float GetMaximumChildOffset(float scrollViewSize, float childSize) =>
        childSize - scrollViewSize;

    private (bool ShouldDisplay, float BarStart, float BarSize) GetScrollBarMetrics(float childStart, float scrollViewSize, float childSize)
    {
        var scrollViewChildSizeRatio = scrollViewSize / childSize;
        var shouldDisplay = scrollViewChildSizeRatio < 1.0f;
        var barStart = -childStart * scrollViewChildSizeRatio;
        var barSize = scrollViewSize * scrollViewChildSizeRatio;

        return (shouldDisplay, barStart, barSize);
    }

    private void OnChildChange(UIWidget? child)
    {
        if (_hasChild) Children.RemoveAt(0);
        if (child != null) Children.Insert(0, child);
        _hasChild = child is not null;
    }

    private class ScrollBar : UIWidget
    {
        private readonly Rectangle _barRectangle;
        private Point? _mouseDragPosition;
        private Point? _delta;

        public Point? Delta => _delta;

        public ScrollBar(Application application) : base(application)
        {
            _barRectangle = new Rectangle(application);
            _barRectangle.BackgroundColor.BindSourceToDestination(BackgroundColor);
            Canvas.Shapes.Add(_barRectangle);

            MouseDragBegin += ScrollBar_MouseDragBegin;
            MouseDrag += ScrollBar_MouseDrag;
            MouseDragEnd += ScrollBar_MouseDragEnd;
            RenderFrame += ScrollBar_RenderFrame;
        }

        private void ScrollBar_RenderFrame(UIComponent sender, RenderingEventArgs e)
        {
            var cornerRadius = _barRectangle.ComputeCornerRadius(1.0f, e.RenderingAreaSize);
            _barRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
            _barRectangle.CornerRadiusX.Value = cornerRadius;
            _barRectangle.CornerRadiusY.Value = cornerRadius;
        }

        private void ScrollBar_MouseDragBegin(UIComponent sender, MouseEventArgs e)
        {
            _mouseDragPosition = e.AbsoluteLocation;
        }

        private void ScrollBar_MouseDrag(UIComponent sender, MouseEventArgs e)
        {
            _delta = e.AbsoluteLocation - _mouseDragPosition!;
        }

        private void ScrollBar_MouseDragEnd(UIComponent sender, EventArgs e)
        {
            _mouseDragPosition = null;
            _delta = null;
        }

        protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
            childrenSize.FirstOrDefault() ?? Size.Zero;
    }
}
