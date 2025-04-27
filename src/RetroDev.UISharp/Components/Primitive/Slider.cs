using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Core.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Exceptions;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A slider where the user can drag the circle to select a numeric value.
/// </summary>
public class Slider : UIControl
{
    private readonly Rectangle _backgroundRectangle;
    private readonly DraggableCircle _sliderCircle;

    /// <summary>
    /// The current slider value.
    /// </summary>
    public UIProperty<int> Value { get; }

    /// <summary>
    /// The minimum allowed <see cref="Value"/>.
    /// </summary>
    public UIProperty<int> MinimumValue { get; }

    /// <summary>
    /// The current progress <see cref="Value"/>.
    /// </summary>
    public UIProperty<int> MaximumValue { get; }

    /// <summary>
    /// The color of the slider circle.
    /// </summary>
    public UIProperty<Color> ForegroundColor { get; }

    /// <summary>
    /// Creates a new slider.
    /// </summary>
    /// <param name="application">The application that contain this progress bar.</param>
    /// <param name="value">The initial slider value.</param>
    public Slider(Application application, int value = 0) : base(application, isFocusable: false)
    {
        Value = new UIProperty<int>(this, value);
        MinimumValue = new UIProperty<int>(this, 0);
        MaximumValue = new UIProperty<int>(this, 100);
        ForegroundColor = CreateNewColorPropertyFor<ProgressBar>(UISharpColorNames.ProgressBarForeground);
        BackgroundColor.BindTheme(UISharpColorNames.ProgressBarBackground);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindSourceToDestination(BackgroundColor);
        Canvas.Shapes.Add(_backgroundRectangle);

        _sliderCircle = new DraggableCircle(application);
        _sliderCircle.BackgroundColor.BindSourceToDestination(ForegroundColor);
        _sliderCircle.AutoHeight.Value = AutoSize.Stretch;
        _sliderCircle.MouseHorizontalPosition.ValueChange.Subscribe(OnSliderDrag);
        Children.Add(_sliderCircle);

        RenderFrame += ProgressBar_RenderFrame;
    }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();
        if (Value.Value < MinimumValue.Value) throw new UIPropertyValidationException($"Value {Value.Value} must be greater or equal to MinimumValue {MinimumValue.Value}", this);
        if (Value.Value > MaximumValue.Value) throw new UIPropertyValidationException($"Value {Value.Value} must be less than or equal to MaximumValue {MaximumValue.Value}", this);
        if (MaximumValue.Value < MinimumValue.Value) throw new UIPropertyValidationException($"MaximumValue {MaximumValue.Value} must be greater or equal to {MinimumValue.Value}", this);
    }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        new Size(childrenSize.First().Width * 5, childrenSize.First().Height);

    /// <inheritdoc />
    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> sizeHints)
    {
        var circleSize = new Size(availableSpace.Height, availableSpace.Height);
        var minimumHorizontalPosition = PixelUnit.Zero;
        var maximumHorizontalPosition = availableSpace.Width - circleSize.Width;
        var percentage = (Value.Value - MinimumValue.Value) / (float)(MaximumValue.Value - MinimumValue.Value);
        var circlePosition = new Point(maximumHorizontalPosition * percentage, PixelUnit.Zero);
        return [new Area(circlePosition, circleSize)];
    }

    private void ProgressBar_RenderFrame(UIObject sender, RenderingEventArgs e)
    {
        var padding = Padding.ToMarginStruct();
        var size = new Size(e.RenderingAreaSize.Width, e.RenderingAreaSize.Height / 5);
        _backgroundRectangle.RelativeRenderingArea.Value = size.PositionCenterOf(e.RenderingAreaSize);
    }

    private void OnSliderDrag(PixelUnit? value)
    {
        if (value is null) return;
        var mouseHorizontalPositionRelativeToSlider = _sliderCircle.ActualRelativeLocation.X.Value + value.Value;
        var percentage = Math.Clamp(mouseHorizontalPositionRelativeToSlider / ActualSize.Width, 0.0f, 1.0f);
        Value.Value = (int)(MinimumValue.Value + percentage * (MaximumValue.Value - MinimumValue.Value));
    }

    private class DraggableCircle : UIControl
    {
        private Circle _circle;

        public UIProperty<PixelUnit?> MouseHorizontalPosition { get; }

        public DraggableCircle(Application application) : base(application)
        {
            MouseHorizontalPosition = new UIProperty<PixelUnit?>(this, (PixelUnit?)null);
            _circle = new Circle(application);
            _circle.BackgroundColor.BindSourceToDestination(BackgroundColor);
            Canvas.Shapes.Add(_circle);

            MouseDrag += DraggableCircle_MouseDrag;
            MouseDragEnd += DraggableCircle_MouseDragEnd;
            RenderFrame += DraggableCircle_RenderFrame;
        }

        private void DraggableCircle_MouseDrag(UIObject sender, MouseEventArgs e)
        {
            MouseHorizontalPosition.Value = e.RelativeLocation.X;
        }

        private void DraggableCircle_MouseDragEnd(UIObject sender, EventArgs e)
        {
            MouseHorizontalPosition.Value = null;
        }

        private void DraggableCircle_RenderFrame(UIObject sender, RenderingEventArgs e)
        {
            _circle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
        }

        protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
            new Size(20, 20);
    }
}
