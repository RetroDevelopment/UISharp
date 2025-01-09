using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;
using SDL2;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A box that allows to enter text.
/// </summary>
public class EditBox : UIComponent
{
    private readonly Label _inputTextLabel;

    public UIProperty<EditBox, string> Text { get; }

    protected override Size ComputeSizeHint() => new(20 * 10, 20); // 20 is font size and 10 the characters (estimate)

    public EditBox(Application parent) : base(parent)
    {
        _inputTextLabel = new Label(parent);
        AddChild(_inputTextLabel);
        Text = new(this, string.Empty);
        _inputTextLabel.Text.AddBinder(new PropertyBinder<EditBox, string>(Text, BindingType.DestinationToSource));
        _inputTextLabel.AutoWidth.Value = AutoSizeStrategy.WrapComponentLeftTop;
        _inputTextLabel.AutoHeight.Value = AutoSizeStrategy.WrapComponentCenter;

        RenderFrame += EditBox_RenderFrame;
        MousePress += EditBox_MousePress;
        KeyPress += EditBox_KeyPress;
        TextInput += EditBox_TextInput;
    }

    // TODO: getting the exact key is more difficult and it will require platform specific libraries. So I will maybe need to create a C++ VS project.
    private void EditBox_KeyPress(UIComponent sender, KeyEventArgs e)
    {
        var key = e.Button;
        if (key == KeyButton.Backspace && !string.IsNullOrEmpty(Text))
        {
            Text.Value = Text.Value.Substring(0, Text.Value.Length - 1);
        }
    }

    private void EditBox_TextInput(UIComponent sender, TextInputEventArgs e)
    {
        Text.Value += e.Text;
    }

    private void EditBox_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Focus.Value = true;
        }
    }

    private void EditBox_RenderFrame(UIComponent sender, RenderingEventArgs e)
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

    }
}
