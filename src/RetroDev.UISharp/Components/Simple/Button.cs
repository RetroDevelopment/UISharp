using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Coordinates;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A clickable button.
/// </summary>
public class Button : UIWidget
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Label _buttonTextLabel;

    /// <summary>
    /// Raised when clicking on the button.
    /// </summary>
    public event TypeSafeEventHandler<Button, EventArgs>? Action;

    /// <summary>
    /// The button text.
    /// </summary>
    public UIProperty<Button, string> Text { get; }

    /// <summary>
    /// The button text font.
    /// </summary>
    public UIProperty<Button, Font> Font { get; }

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
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.First();

    /// <summary>
    /// Creates a new button.
    /// </summary>
    /// <param name="application">The application that contains this button.</param>
    /// <param name="text">The button text.</param>
    public Button(Application application, string text = "") : base(application)
    {
        Text = new UIProperty<Button, string>(this, text ?? string.Empty);
        Font = new UIProperty<Button, Font>(this, Application.DefaultFont, BindingType.DestinationToSource);
        TextColor = new UIProperty<Button, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        DisabledTextColor = new UIProperty<Button, Color>(this, Application.Theme.TextColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<Button, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<Button, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);

        BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColor);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BorderColor.BindDestinationToSource(FocusColor);
        Canvas.Add(_backgroundRectangle);

        _buttonTextLabel = new Label(application);
        _buttonTextLabel.Text.BindDestinationToSource(Text);
        _buttonTextLabel.Font.BindDestinationToSource(Font);
        _buttonTextLabel.TextColor.BindDestinationToSource(TextColor);
        UpdateTextColor();
        AddChildNode(_buttonTextLabel);

        MousePress += Button_MousePress; // TODO: managing button action is more complicated than intercepting key press events.
        RenderFrame += Button_RenderFrame;
        Enabled.ValueChange += (_, _) => UpdateTextColor();
    }

    private void Button_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Focus.Value = true;
            Action?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Button_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
        var cornerRadius = _backgroundRectangle.ComputeCornerRadius(0.5f, e.RenderingAreaSize);
        _backgroundRectangle.CornerRadiusX.Value = cornerRadius;
        _backgroundRectangle.CornerRadiusY.Value = cornerRadius;
        UpdateBackgroundRectangleColor();
        UpdateBackgroundRectangleBorder();
    }

    private void UpdateBackgroundRectangleColor()
    {
        if (Enabled.Value)
        {
            _backgroundRectangle.BackgroundColor.Value = BackgroundColor.Value;
        }
        else
        {
            _backgroundRectangle.BackgroundColor.Value = DisabledBackgroundColor.Value;
        }
    }

    private void UpdateTextColor()
    {
        if (Enabled.Value)
        {
            _buttonTextLabel.TextColor.Value = TextColor.Value;
        }
        else
        {
            _buttonTextLabel.TextColor.Value = DisabledTextColor.Value;
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
