using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Core.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A clickable button.
/// </summary>
public class Button : UIControl
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Label _buttonTextLabel;

    private readonly UIProperty<bool> _isMouseHover;
    private readonly UIProperty<bool> _isAction;

    /// <summary>
    /// Raised when clicking on the button.
    /// </summary>
    public event TypeSafeEventHandler<Button, EventArgs>? Action;

    /// <summary>
    /// The button text.
    /// </summary>
    public UIProperty<string> Text { get; }

    /// <summary>
    /// The button text font.
    /// </summary>
    public UIProperty<Font> Font { get; }

    /// <summary>
    /// The text color.
    /// </summary>
    public UIProperty<Color> TextColor { get; }

    /// <summary>
    /// The text color when the button is disabled.
    /// </summary>
    public UIProperty<Color> DisabledTextColor { get; }

    /// <summary>
    /// The background color when the button is disabled.
    /// </summary>
    public UIProperty<Color> DisabledBackgroundColor { get; }

    /// <summary>
    /// The color indicating that the button is focused.
    /// </summary>
    public UIProperty<Color> FocusColor { get; }

    /// <summary>
    /// The button border color.
    /// </summary>
    public UIProperty<Color> BorderColor { get; }

    /// <summary>
    /// The color when the mouse is within the button.
    /// </summary>
    public UIProperty<Color> HoverColor { get; }

    /// <summary>
    /// The color when clicking on the button.
    /// </summary>
    public UIProperty<Color> ActionColor { get; }

    /// <summary>
    /// The button text horizontal alignment.
    /// </summary>
    public UIProperty<IHorizontalAlignment> TextHorizontalAlignment { get; }

    /// <summary>
    /// The button text vertical alignment.
    /// </summary>
    public UIProperty<IVerticalAlignment> TextVerticalAlignment { get; }

    /// <summary>
    /// Creates a new button.
    /// </summary>
    /// <param name="application">The application that contains this button.</param>
    /// <param name="text">The button text.</param>
    public Button(Application application, string text = "") : base(application)
    {
        _isMouseHover = new UIProperty<bool>(this, false);
        _isAction = new UIProperty<bool>(this, false);

        Text = new UIProperty<string>(this, text ?? string.Empty);
        Font = new UIProperty<Font>(this, Application.DefaultFont, BindingType.SourceToDestination);
        TextColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonText);
        DisabledTextColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonDisabledText);
        FocusColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonFocusBorder);
        DisabledBackgroundColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonDisabled);
        BorderColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonBorder);
        HoverColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonHover);
        ActionColor = CreateNewColorPropertyFor<Button>(UISharpColorNames.ButtonAction);
        TextHorizontalAlignment = new UIProperty<IHorizontalAlignment>(this, Alignment.Center);
        TextVerticalAlignment = new UIProperty<IVerticalAlignment>(this, Alignment.Center);

        BackgroundColor.BindTheme(UISharpColorNames.ButtonBackground);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BorderThickness.Value = 3.0f; // TODO: use styles
        Canvas.Shapes.Add(_backgroundRectangle);

        _buttonTextLabel = new Label(application);
        _buttonTextLabel.Text.BindSourceToDestination(Text);
        _buttonTextLabel.Font.BindSourceToDestination(Font);
        _buttonTextLabel.TextColor.BindSourceToDestination(TextColor);
        _buttonTextLabel.HorizontalAlignment.BindSourceToDestination(TextHorizontalAlignment);
        _buttonTextLabel.VerticalAlignment.BindSourceToDestination(TextVerticalAlignment);
        _buttonTextLabel.Margin.BindSourceToDestination(Padding);
        Children.Add(_buttonTextLabel);

        KeyPress += Button_KeyPress;
        MousePress += Button_MousePress;
        MouseEnter += Button_MouseEnter;
        MouseLeave += Button_MouseLeave;
        MouseDragEnd += Button_MouseDragEnd;
        RenderFrame += Button_RenderFrame;
        Enabled.ValueChange.Subscribe(_ => UpdateTextColor());
    }

    /// <summary>
    /// Simulates the button press action, by triggering the <see cref="Action"/> event.
    /// </summary>
    public void Press()
    {
        Action?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.First();

    private void Button_KeyPress(UIObject sender, KeyEventArgs e)
    {
        if (e.Button == KeyButton.Return || e.Button == KeyButton.KeyPadEnter)
        {
            Press();
        }
    }

    private void Button_MousePress(UIObject sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Focus.Value = true;
            _isAction.Value = true;
        }
    }

    private void Button_MouseEnter(UIObject sender, EventArgs e)
    {
        _isMouseHover.Value = true;
    }

    private void Button_MouseLeave(UIObject sender, EventArgs e)
    {
        _isMouseHover.Value = false;
    }

    private void Button_MouseDragEnd(UIObject sender, EventArgs e)
    {
        if (_isMouseHover.Value)
        {
            Action?.Invoke(this, EventArgs.Empty);
        }

        _isAction.Value = false;
    }

    private void Button_RenderFrame(UIObject sender, RenderingEventArgs e)
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
        if (Enabled.Value && _isAction.Value)
        {
            _backgroundRectangle.BackgroundColor.Value = ActionColor.Value;
        }
        else if (Enabled.Value && _isMouseHover.Value)
        {
            _backgroundRectangle.BackgroundColor.Value = HoverColor.Value;
        }
        else if (Enabled.Value)
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
            _backgroundRectangle.BorderColor.BindSourceToDestination(FocusColor);
        }
        else
        {
            _backgroundRectangle.BorderColor.BindSourceToDestination(BorderColor);
        }
    }
}
