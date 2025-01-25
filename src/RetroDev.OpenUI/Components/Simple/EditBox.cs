using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A box that allows to enter text.
/// </summary>
public class EditBox : UIComponent
{
    private readonly Rectangle _backgroundRectangle;
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
    /// <param name="application">The application that contains this button.</param>
    public EditBox(Application application) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap, horizontalAlignment: Alignment.Left, verticalAlignment: Alignment.Center)
    {
        Text = new UIProperty<EditBox, string>(this, string.Empty);
        TextColor = new UIProperty<EditBox, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        DisabledTextColor = new UIProperty<EditBox, Color>(this, Application.Theme.TextColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<EditBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<EditBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        BackgroundColor.BindDestinationToSource(Application.Theme.PrimaryBackground);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        _backgroundRectangle.BorderColor.BindDestinationToSource(Application.Theme.BorderColor);
        _backgroundRectangle.AutoCornerRadiusRatio.Value = 0.5f;
        UpdateBackgroundRectangleColorBinding();
        UpdateBackgroundRectangleBorder();
        AddChild(_backgroundRectangle);

        _inputTextLabel = new Label(application);
        _inputTextLabel.Text.BindDestinationToSource(Text);
        _inputTextLabel.AutoWidth.Value = AutoSize.Wrap;
        _inputTextLabel.AutoHeight.Value = AutoSize.Wrap;
        _inputTextLabel.HorizontalAlignment.Value = Alignment.Left;
        _inputTextLabel.VerticalAlignment.Value = Alignment.Center;
        UpdateTextColorBinding();
        AddChild(_inputTextLabel);

        MousePress += EditBox_MousePress;
        KeyPress += EditBox_KeyPress;
        TextInput += EditBox_TextInput;
        Enabled.ValueChange += Enabled_ValueChange;
        Focus.ValueChange += Focus_ValueChange;
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

    private void Enabled_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateBackgroundRectangleColorBinding();
        UpdateTextColorBinding();
    }

    private void Focus_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateBackgroundRectangleBorder();
    }

    private void UpdateBackgroundRectangleColorBinding()
    {
        if (Enabled.Value)
        {
            _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        }
        else
        {
            _backgroundRectangle.BackgroundColor.BindDestinationToSource(DisabledBackgroundColor);
        }
    }

    private void UpdateTextColorBinding()
    {
        if (Enabled.Value)
        {
            _inputTextLabel.TextColor.BindDestinationToSource(TextColor);
        }
        else
        {
            _inputTextLabel.TextColor.BindDestinationToSource(DisabledTextColor);
        }
    }

    private void UpdateBackgroundRectangleBorder()
    {
        if (Focus.Value)
        {
            _backgroundRectangle.BorderThickness.Value = 5.0f;
        }
        else
        {
            _backgroundRectangle.BorderThickness.Value = 0.0f;
        }
    }

}
