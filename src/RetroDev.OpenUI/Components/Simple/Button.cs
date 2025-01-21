using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.Themes;

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

    /// <summary>
    /// The text color.
    /// </summary>
    public UIProperty<Button, Color> TextColor { get; }

    /// <summary>
    /// The text color when the button is disabled.
    /// </summary>
    public UIProperty<Button, Color> DisabledTextColor { get; }

    /// <summary>
    /// The background color when the button is disabled.
    /// </summary>
    public UIProperty<Button, Color> DisabledBackgroundColor { get; }

    /// <summary>
    /// The color indicating that the button is focused.
    /// </summary>
    public UIProperty<Button, Color> FocusColor { get; }

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() => _buttonTextLabel.SizeHint;

    /// <summary>
    /// Creates a new button.
    /// </summary>
    /// <param name="parent">The application that contains this button.</param>
    public Button(Application parent) : base(parent)
    {
        Text = new UIProperty<Button, string>(this, string.Empty);
        TextColor = new UIProperty<Button, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        DisabledTextColor = new UIProperty<Button, Color>(this, Application.Theme.TextColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<Button, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<Button, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);

        _buttonTextLabel = new Label(parent);
        _buttonTextLabel.Text.ValueChange += (_, _) => SizeHintCache.MarkDirty();
        _buttonTextLabel.Text.BindDestinationToSource(Text);
        BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColor);

        UpdateTextColorBinding();
        Enabled.ValueChange += Enabled_ValueChange;

        RenderFrame += Button_RenderFrame;
        MousePress += Button_MousePress; // TODO: managing button action is more complicated than intercepting key press events.
        AddChild(_buttonTextLabel);
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

    private void Enabled_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateTextColorBinding();
    }

    private void UpdateTextColorBinding()
    {
        if (Enabled)
        {
            _buttonTextLabel.TextColor.BindDestinationToSource(TextColor);
        }
        else
        {
            _buttonTextLabel.TextColor.BindDestinationToSource(DisabledTextColor);
        }
    }
}
