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
    /// The color of the border displayed when the button is focused.
    /// </summary>
    public UIProperty<Button, Color> BorderColor { get; }

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
        TextColor = new UIProperty<Button, Color>(this, Theme.DefaultColor);
        DisabledTextColor = new UIProperty<Button, Color>(this, Theme.DefaultColor);
        BorderColor = new UIProperty<Button, Color>(this, Theme.DefaultColor);
        DisabledBackgroundColor = new UIProperty<Button, Color>(this, Theme.DefaultColor);

        _buttonTextLabel.Text.AddBinder(new PropertyBinder<Button, string>(Text, BindingType.DestinationToSource));
        TextColor.AddBinder(new PropertyBinder<Theme, Color>(Application.Theme.TextColor, BindingType.DestinationToSource));
        DisabledTextColor.AddBinder(new PropertyBinder<Theme, Color>(Application.Theme.TextColorDisabled, BindingType.DestinationToSource));
        BackgroundColor.AddBinder(new PropertyBinder<Theme, Color>(Application.Theme.PrimaryColor, BindingType.DestinationToSource));
        DisabledBackgroundColor.AddBinder(new PropertyBinder<Theme, Color>(Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource));
        BorderColor.AddBinder(new PropertyBinder<Theme, Color>(Application.Theme.BorderColor, BindingType.DestinationToSource));

        UpdateTextColorBinding();
        Enabled.ValueChange += Enabled_ValueChange;

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
        float minimumDimension = Math.Min(size.Width, size.Height);
        PixelUnit cornerRadius = (minimumDimension / 2.0f) * 0.5f;

        Color rectangleBackgroundColor = Enabled ? BackgroundColor : DisabledBackgroundColor;
        var backgroundRectangle = new Rectangle(BackgroundColor: rectangleBackgroundColor, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius);
        canvas.Render(backgroundRectangle, RelativeDrawingArea.Fill());

        if (Focus.Value)
        {
            var borderRectangle = new Rectangle(BorderColor: BorderColor, BorderThickness: 5.0f, CornerRadiusX: cornerRadius, CornerRadiusY: cornerRadius);
            canvas.Render(borderRectangle, RelativeDrawingArea.Fill());
        }
    }

    private void Enabled_ValueChange(UIComponent sender, ValueChangeEventArgs<bool> e)
    {
        UpdateTextColorBinding();
    }

    private void UpdateTextColorBinding()
    {
        _buttonTextLabel.TextColor.RemoveBinders();
        if (Enabled)
        {
            _buttonTextLabel.TextColor.AddBinder(new PropertyBinder<Button, Color>(TextColor, BindingType.DestinationToSource));
        }
        else
        {
            _buttonTextLabel.TextColor.AddBinder(new PropertyBinder<Button, Color>(DisabledTextColor, BindingType.DestinationToSource));
        }
    }
}
