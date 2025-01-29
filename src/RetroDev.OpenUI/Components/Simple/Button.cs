using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A clickable button.
/// </summary>
public class Button : UIComponent
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Label _buttonTextLabel;

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
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.ElementAt(1);

    /// <summary>
    /// Creates a new button.
    /// </summary>
    /// <param name="application">The application that contains this button.</param>
    public Button(Application application) : base(application)
    {
        Text = new UIProperty<Button, string>(this, string.Empty);
        TextColor = new UIProperty<Button, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        DisabledTextColor = new UIProperty<Button, Color>(this, Application.Theme.TextColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<Button, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<Button, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);
        BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColor);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        _backgroundRectangle.BorderColor.BindDestinationToSource(Application.Theme.BorderColor);
        _backgroundRectangle.AutoCornerRadiusRatio.Value = 0.5f;
        UpdateBackgroundRectangleColorBinding();
        UpdateBackgroundRectangleBorder();
        AddChild(_backgroundRectangle);

        _buttonTextLabel = new Label(application);
        _buttonTextLabel.Text.BindDestinationToSource(Text);
        UpdateTextColorBinding();
        AddChild(_buttonTextLabel);

        MousePress += Button_MousePress; // TODO: managing button action is more complicated than intercepting key press events.
        Enabled.ValueChange += Enabled_ValueChange;
        Focus.ValueChange += Focus_ValueChange;
    }

    private void Button_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Focus.Value = true;
            Action.Invoke(this, EventArgs.Empty);
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
            _buttonTextLabel.TextColor.BindDestinationToSource(TextColor);
        }
        else
        {
            _buttonTextLabel.TextColor.BindDestinationToSource(DisabledTextColor);
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
