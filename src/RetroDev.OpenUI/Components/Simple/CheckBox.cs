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
    protected override Size ComputeSizeHint() => new(20, 20); // TODO: Maybe same size as default label text size (which is 20).

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

        if (Focus.Value)
        {
            canvas.Render(new Rectangle(new(0, 100, 0, 255)), new(Point.Zero, size));
        }
        else if (Enabled)
        {
            canvas.Render(new Rectangle(new(0, 0, 100, 255)), new(Point.Zero, size));
        }
        else
        {
            canvas.Render(new Rectangle(new(100, 100, 100, 255)), new(Point.Zero, size));
        }

        if (Checked)
        {
            canvas.Render(new Circle(new(100, 0, 0, 255)), new(Point.Zero, size));
        }
    }

    private void CheckBox_MousePress(UIComponent sender, Events.MouseEventArgs e)
    {
        Checked.Value = !Checked;
        Focus.Value = true;
    }
}
