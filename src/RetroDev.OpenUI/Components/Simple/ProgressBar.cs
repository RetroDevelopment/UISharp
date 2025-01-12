using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

// TODO: add colors and font size

/// <summary>
/// A bar displaying progress.
/// </summary>
public class ProgressBar : UIComponent
{
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

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() => new(100, 20); // TODO: 20 is the common text size, 100 is some value to be big enough. Make sure that the size fits the screen.

    /// <inheritdoc />
    protected override bool DefaultIsFocusable => false;

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="parent">The application that contain this progress bar.</param>
    public ProgressBar(Application parent) : base(parent)
    {
        Value = new UIProperty<ProgressBar, int>(this, 0);
        MinimumValue = new UIProperty<ProgressBar, int>(this, 0);
        MaximumValue = new UIProperty<ProgressBar, int>(this, 100);
        RenderFrame += Label_RenderFrame;
    }

    /// <inheritdoc />
    protected override void ValidateImplementation()
    {
        if (Value.Value < MinimumValue.Value) throw new UIPropertyValidationException($"Value {Value.Value} must be greater or equal to MinimumValue {MinimumValue.Value}", this);
        if (Value.Value > MaximumValue.Value) throw new UIPropertyValidationException($"Value {Value.Value} must be less than or equal to MaximumValue {MaximumValue.Value}", this);
        if (MaximumValue.Value < MinimumValue.Value) throw new UIPropertyValidationException($"MaximumValue {MaximumValue.Value} must be greater or equal to {MinimumValue.Value}", this);
    }

    private void Label_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var canvas = e.Canvas;

        var size = RelativeDrawingArea.Size;
        var value = Math.Clamp(Value, MinimumValue, MaximumValue);
        var percentage = (value - MinimumValue) / (float)(MaximumValue - MinimumValue);
        var width = size.Width * percentage;

        canvas.Render(new Rectangle(new(255, 255, 255, 255)), new Area(Point.Zero, size));
        canvas.Render(new Rectangle(new(255, 0, 0, 255)), new Area(Point.Zero, new Size(width, size.Height)));
    }
}
