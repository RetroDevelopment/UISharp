using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
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

    protected override bool DefaultIsFocusable => false;

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="parent">The application that contain this progress bar.</param>
    public ProgressBar(Application parent) : base(parent)
    {
        Value = new(this, 0);
        MinimumValue = new(this, 0);
        MaximumValue = new(this, 100);
        RenderFrame += Label_RenderFrame;
    }

    private void Label_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var canvas = e.Canvas;

        ValidateState();
        var size = RelativeDrawingArea.Size;
        var value = Math.Clamp(Value, MinimumValue, MaximumValue);
        var percentage = (value - MinimumValue) / (float)(MaximumValue - MinimumValue);
        var width = size.Width * percentage;

        canvas.Render(new Rectangle(new(255, 255, 255, 255)), new(Point.Zero, size));
        canvas.Render(new Rectangle(new(255, 0, 0, 255)), new Area(Point.Zero, new Size(width, size.Height)));
    }

    private void ValidateState()
    {
        if (MinimumValue > MaximumValue) throw new InvalidOperationException($"Progress bar minimum value ({MinimumValue.Value}) cannot be greater than maximum value ({MaximumValue.Value})");
    }
}
