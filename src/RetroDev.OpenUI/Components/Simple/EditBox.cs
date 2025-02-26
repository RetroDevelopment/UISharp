﻿using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.Presentation;
using RetroDev.OpenUI.Presentation.Properties;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A box that allows to enter text.
/// </summary>
public class EditBox : UIWidget
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

    /// <summary>
    /// The font of the edited text.
    /// </summary>
    public UIProperty<EditBox, Font> Font { get; }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        if (Root == null) return Size.Zero;
        var height = _inputTextLabel.ComputeTextMaximumHeight();
        var estimatedNumberOfCharacters = 10;
        return new Size(height * estimatedNumberOfCharacters, height);
    }

    /// <summary>
    /// Creates a new edit box to insert text.
    /// </summary>
    /// <param name="application">The application that contains this button.</param>
    public EditBox(Application application) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Text = new UIProperty<EditBox, string>(this, string.Empty);
        Font = new UIProperty<EditBox, Font>(this, application.DefaultFont, BindingType.DestinationToSource);
        TextColor = new UIProperty<EditBox, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        DisabledTextColor = new UIProperty<EditBox, Color>(this, Application.Theme.TextColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<EditBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<EditBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);
        BackgroundColor.BindDestinationToSource(Application.Theme.PrimaryBackground);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BorderColor.BindDestinationToSource(FocusColor);
        Canvas.Add(_backgroundRectangle);

        _inputTextLabel = new Label(application);
        _inputTextLabel.Text.BindDestinationToSource(Text);
        _inputTextLabel.Font.BindDestinationToSource(Font);
        _inputTextLabel.AutoWidth.Value = AutoSize.Wrap;
        _inputTextLabel.AutoHeight.Value = AutoSize.Wrap;
        _inputTextLabel.HorizontalAlignment.Value = Alignment.Left;
        _inputTextLabel.VerticalAlignment.Value = Alignment.Center;
        AddChildNode(_inputTextLabel);

        MousePress += EditBox_MousePress;
        KeyPress += EditBox_KeyPress;
        TextInput += EditBox_TextInput;
        Enabled.ValueChange += Enabled_ValueChange;
        Focus.ValueChange += Focus_ValueChange;
        RenderFrame += EditBox_RenderFrame;
    }

    private void EditBox_KeyPress(UIComponent sender, KeyEventArgs e)
    {
        var key = e.Button;
        if (key == KeyButton.Backspace && !string.IsNullOrEmpty(Text.Value))
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
        UpdateTextColorBinding();
    }

    private void Focus_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateBackgroundRectangleBorder();
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

    private void EditBox_RenderFrame(UIComponent sender, RenderingEventArgs e)
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
