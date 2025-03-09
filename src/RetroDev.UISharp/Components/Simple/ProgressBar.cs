using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Exceptions;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A bar displaying progress.
/// </summary>
public class ProgressBar : UIWidget
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Rectangle _progressRectangle;

    /// <summary>
    /// The current progress value.
    /// The progress bar will be filled on the percentage that <see cref="Value"/> is with respect to <see cref="MinimumValue"/> and <see cref="MaximumValue"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If <see cref="Value"/> is not in the [<see cref="MinimumValue"/>, <see cref="MaximumValue"/>] interval.</exception>
    public UIProperty<ProgressBar, int> Value { get; }

    /// <summary>
    /// The minimum allowed <see cref="Value"/>.
    /// </summary>
    public UIProperty<ProgressBar, int> MinimumValue { get; }

    /// <summary>
    /// The current progress <see cref="Value"/>.
    /// </summary>
    public UIProperty<ProgressBar, int> MaximumValue { get; }

    /// <summary>
    /// The color of the bar indicating progress.
    /// </summary>
    public UIProperty<ProgressBar, Color> ForegroundColor { get; }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(100, 20); // TODO: 20 is the common text size, 100 is some value to be big enough. Make sure that the size fits the screen.

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="application">The application that contain this progress bar.</param>
    /// <param name="value">The initial progress value.</param>
    public ProgressBar(Application application, int value = 0) : base(application, isFocusable: false)
    {
        Value = new UIProperty<ProgressBar, int>(this, value);
        MinimumValue = new UIProperty<ProgressBar, int>(this, 0);
        MaximumValue = new UIProperty<ProgressBar, int>(this, 100);
        ForegroundColor = CreateNewColorPropertyFor<ProgressBar>(UISharpColorNames.ProgressBarForeground);
        BackgroundColor.BindTheme(UISharpColorNames.ProgressBarBackground);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        Canvas.Add(_backgroundRectangle);

        _progressRectangle = new Rectangle(application);
        _progressRectangle.BackgroundColor.BindDestinationToSource(ForegroundColor);
        Canvas.Add(_progressRectangle);

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

    private void ProgressBar_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var padding = Padding.ToMarginStruct();

        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
        var backgroundCornerRadius = _backgroundRectangle.ComputeCornerRadius(1.0f, _backgroundRectangle.RelativeRenderingArea.Value.Size);
        _backgroundRectangle.CornerRadiusX.Value = backgroundCornerRadius;
        _backgroundRectangle.CornerRadiusY.Value = backgroundCornerRadius;

        var value = Math.Clamp(Value.Value, MinimumValue.Value, MaximumValue.Value);
        var percentage = (value - MinimumValue.Value) / (float)(MaximumValue.Value - MinimumValue.Value);
        var paddedRenderingArea = e.RenderingAreaSize.Fill().Clamp(e.RenderingAreaSize, padding);
        var progressRectangleWidth = paddedRenderingArea.Size.Width * percentage;
        var progressRecangleSize = new Size(progressRectangleWidth, paddedRenderingArea.Size.Height);
        _progressRectangle.RelativeRenderingArea.Value = paddedRenderingArea;
        _progressRectangle.ClipArea.Value = new Area(paddedRenderingArea.TopLeft, progressRecangleSize);
        var progressCornerRadius = _progressRectangle.ComputeCornerRadius(1.0f, _progressRectangle.RelativeRenderingArea.Value.Size);
        _progressRectangle.CornerRadiusX.Value = progressCornerRadius;
        _progressRectangle.CornerRadiusY.Value = progressCornerRadius;
    }
}
