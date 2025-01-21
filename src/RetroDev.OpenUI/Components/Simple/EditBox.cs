using RetroDev.OpenUI.Components.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.Themes;
using SDL2;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A box that allows to enter text.
/// </summary>
public class EditBox : UIComponent
{
    private readonly Label _inputTextLabel;

    /// <summary>
    /// The edit text.
    /// </summary>
    public UIProperty<EditBox, string> Text { get; }

    /// <summary>
    /// The component text color.
    /// </summary>
    public UIProperty<EditBox, Color> TextColor { get; }

    /// <summary>
    /// The component text color when the edit box is disabled.
    /// </summary>
    public UIProperty<EditBox, Color> DisabledTextColor { get; }

    /// <summary>
    /// The color of the edit box is focused.
    /// </summary>
    public UIProperty<EditBox, Color> FocusColor { get; }

    /// <summary>
    /// The background color when the component is disabled.
    /// </summary>
    public UIProperty<EditBox, Color> DisabledBackgroundColor { get; }

    /// <inheritdoc />
    protected override Size ComputeSizeHint() => new(20 * 10, 20); // 20 is font size and 10 the characters (estimate)

    /// <summary>
    /// Creates a new edit box to insert text.
    /// </summary>
    /// <param name="parent">The application that contains this button.</param>
    public EditBox(Application parent) : base(parent, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap, horizontalAlignment: Alignment.Left, verticalAlignment: Alignment.Center)
    {
        _inputTextLabel = new Label(parent);
        AddChild(_inputTextLabel);
        Text = new UIProperty<EditBox, string>(this, string.Empty);
        TextColor = new UIProperty<EditBox, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        DisabledTextColor = new UIProperty<EditBox, Color>(this, Application.Theme.TextColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<EditBox, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<EditBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);

        _inputTextLabel.Text.BindDestinationToSource(Text);
        _inputTextLabel.AutoWidth.Value = AutoSize.Wrap;
        _inputTextLabel.AutoHeight.Value = AutoSize.Wrap;
        _inputTextLabel.HorizontalAlignment.Value = Alignment.Left;
        _inputTextLabel.VerticalAlignment.Value = Alignment.Center;

        BackgroundColor.BindDestinationToSource(Application.Theme.PrimaryBackground);

        UpdateTextColorBinding();
        Enabled.ValueChange += Enabled_ValueChange;
        RenderFrame += EditBox_RenderFrame;
        MousePress += EditBox_MousePress;
        KeyPress += EditBox_KeyPress;
        TextInput += EditBox_TextInput;
    }

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
        float minimumDimension = Math.Min(size.Width, size.Height);
        PixelUnit cornerRadius = (minimumDimension / 2.0f) * 0.5f;

        Color rectangleBackgroundColor = Enabled ? BackgroundColor : DisabledBackgroundColor;
        var backgroundRectangle = new Rectangle(BackgroundColor: rectangleBackgroundColor, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius);
        canvas.Render(backgroundRectangle, RelativeDrawingArea.Fill());

        if (Focus.Value)
        {
            var borderRectangle = new Rectangle(BorderColor: FocusColor, BorderThickness: 5.0f, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius);
            canvas.Render(borderRectangle, RelativeDrawingArea.Fill());
        }
    }

    private void UpdateTextColorBinding()
    {
        if (Enabled)
        {
            _inputTextLabel.TextColor.BindDestinationToSource(TextColor);
        }
        else
        {
            _inputTextLabel.TextColor.BindDestinationToSource(DisabledTextColor);
        }
    }

    private void Enabled_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateTextColorBinding();
    }
}
