using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A clickable button.
/// </summary>
public class Button : UIComponent
{
    private Label _buttonTextLabel;

    /// <summary>
    /// Raised when clicking on the button.
    /// </summary>
    public event TypeSafeEventHandler<Button, EventArgs> Action = (_, _) => { };

    /// <summary>
    /// The button text.
    /// </summary>
    public UIProperty<Button, string> Text { get; }

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() => _buttonTextLabel.SizeHint;

    /// <summary>
    /// Creates a new button.
    /// </summary>
    /// <param name="parent">The application that contains this button.</param>
    public Button(Application parent) : base(parent)
    {
        _buttonTextLabel = new Label(parent);
        _buttonTextLabel.Text.ValueChange += (_, _) => SizeHintCache.MarkDirty();
        AddChild(_buttonTextLabel);
        Text = new UIProperty<Button, string>(this, string.Empty);
        _buttonTextLabel.Text.AddBinder(new PropertyBinder<Button, string>(Text, BindingType.DestinationToSource));

        RenderFrame += Button_RenderFrame;
        MousePress += Button_MousePress; // TODO: managing button action is more complicated than intercepting key press events.
    }

    private void Button_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Focus.Value = true;
            Action.Invoke(this, EventArgs.Empty);
        }
    }

    private void Button_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var size = RelativeDrawingArea.Size;
        var canvas = e.Canvas;

        if (Focus.Value)
        {
            canvas.Render(new Rectangle(new Color(0, 100, 0, 255)), new(Point.Zero, size));
        }
        else if (Enabled)
        {
            canvas.Render(new Rectangle(new Color(0, 0, 100, 255)), new(Point.Zero, size));
        }
        else
        {
            canvas.Render(new Rectangle(new Color(100, 100, 100, 255)), new(Point.Zero, size));
        }
    }
}
