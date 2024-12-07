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
    private bool _isShiftPressed = false;
    private bool _upperCase = false;
    private readonly Label _inputTextLabel;
    // TODO: remove this once setting up the actual way to get a key
    private readonly Dictionary<KeyButton, string> _displayableCharacters = new Dictionary<KeyButton, string>
{
    { KeyButton.Space, " " },
    { KeyButton.Exclaim, "!" },
    { KeyButton.Quote, "\"" },
    { KeyButton.Hash, "#" },
    { KeyButton.Dollar, "$" },
    { KeyButton.Ampersand, "&" },
    { KeyButton.LeftParen, "(" },
    { KeyButton.RightParen, ")" },
    { KeyButton.Asterisk, "*" },
    { KeyButton.Plus, "+" },
    { KeyButton.Comma, "," },
    { KeyButton.Minus, "-" },
    { KeyButton.Period, "." },
    { KeyButton.Slash, "/" },
    { KeyButton.Num0, "0" },
    { KeyButton.Num1, "1" },
    { KeyButton.Num2, "2" },
    { KeyButton.Num3, "3" },
    { KeyButton.Num4, "4" },
    { KeyButton.Num5, "5" },
    { KeyButton.Num6, "6" },
    { KeyButton.Num7, "7" },
    { KeyButton.Num8, "8" },
    { KeyButton.Num9, "9" },
    { KeyButton.A, "A" },
    { KeyButton.B, "B" },
    { KeyButton.C, "C" },
    { KeyButton.D, "D" },
    { KeyButton.E, "E" },
    { KeyButton.F, "F" },
    { KeyButton.G, "G" },
    { KeyButton.H, "H" },
    { KeyButton.I, "I" },
    { KeyButton.J, "J" },
    { KeyButton.K, "K" },
    { KeyButton.L, "L" },
    { KeyButton.M, "M" },
    { KeyButton.N, "N" },
    { KeyButton.O, "O" },
    { KeyButton.P, "P" },
    { KeyButton.Q, "Q" },
    { KeyButton.R, "R" },
    { KeyButton.S, "S" },
    { KeyButton.T, "T" },
    { KeyButton.U, "U" },
    { KeyButton.V, "V" },
    { KeyButton.W, "W" },
    { KeyButton.X, "X" },
    { KeyButton.Y, "Y" },
    { KeyButton.Z, "Z" }
};
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
        KeyRelease += EditBox_KeyRelease;
    }

    // TODO: getting the exact key is more difficult and it will require platform specific libraries. So I will maybe need to create a C++ VS project.
    private void EditBox_KeyPress(UIComponent sender, KeyEventArgs e)
    {
        var key = e.Button;
        if (_displayableCharacters.TryGetValue(key, out var value))
        {
            var caseCorrectedValue = _upperCase ? value : value.ToLowerInvariant();
            Text.Value = Text.Value + caseCorrectedValue;
        }
        else if (key == KeyButton.Backspace && !string.IsNullOrEmpty(Text))
        {
            Text.Value = Text.Value.Substring(0, Text.Value.Length - 1);
        }
        else if (key == KeyButton.LeftShift || key == KeyButton.RightShift)
        {
            _upperCase = true;
        }
    }

    private void EditBox_KeyRelease(UIComponent sender, KeyEventArgs e)
    {
        var key = e.Button;
        if (key == KeyButton.LeftShift || key == KeyButton.RightShift)
        {
            _upperCase = false;
        }
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
