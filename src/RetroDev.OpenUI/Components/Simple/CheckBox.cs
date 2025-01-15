using RetroDev.OpenUI.Components.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

// TODO: add colors and font size

/// <summary>
/// A checkbox to mark with a tick if something is checked.
/// </summary>
public class CheckBox : UIComponent
{
    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>
    public UIProperty<CheckBox, bool> Checked { get; }

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() => new(80, 30); // TODO: Maybe same size as default label text size (which is 20).

    /// <inheritdoc />
    protected override IAutoSize DefaultAutoWidth => AutoSize.Wrap;

    /// <inheritdoc />
    protected override IAutoSize DefaultAutoHeight => AutoSize.Wrap;

    /// <inheritdoc />
    protected override IHorizontalAlignment DefaultHorizontalAlignment => Alignment.Center;

    /// <inheritdoc />
    protected override IVerticalAlignment DefaultVerticalAlignment => Alignment.Center;


    /// <summary>
    /// Creates a new checkbox.
    /// </summary>
    /// <param name="parent">The application that contain this checkbox.</param>
    public CheckBox(Application parent) : base(parent)
    {
        Checked = new UIProperty<CheckBox, bool>(this, false);
        RenderFrame += CheckBox_RenderFrame;
        MousePress += CheckBox_MousePress;
    }

    private void CheckBox_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var size = RelativeDrawingArea.Size;
        var canvas = e.Canvas;
        float minimumDimension = Math.Min(size.Width, size.Height);
        PixelUnit cornerRadius = minimumDimension / 2.0f;
        var circleRadius = size.Height;

        if (Enabled)
        {
            canvas.Render(new Rectangle(new(0, 0, 100, 255), CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius), new(Point.Zero, size));
        }
        else
        {
            canvas.Render(new Rectangle(new(100, 100, 100, 255), CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius), new(Point.Zero, size));
        }

        if (Checked)
        {
            canvas.Render(new Circle(new(100, 0, 0, 255)), new(new Point(size.Width - circleRadius, 0.0f), new Size(size.Height, size.Height)));
        }
        else
        {
            canvas.Render(new Circle(new(100, 0, 0, 255)), new(Point.Zero, new Size(size.Height, size.Height)));
        }

        if (Focus.Value)
        {
            canvas.Render(new Rectangle(BorderColor: new Color(255, 255, 255, 255), BorderThickness: 5.0f, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius), new(Point.Zero, size));
        }
    }

    private void CheckBox_MousePress(UIComponent sender, Events.MouseEventArgs e)
    {
        Checked.Value = !Checked;
        Focus.Value = true;
    }
}
