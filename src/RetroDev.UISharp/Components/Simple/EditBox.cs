using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A box that allows to enter text.
/// </summary>
public class EditBox : UIWidget
{
    private readonly EditableTextBuffer _textBuffer;

    private readonly Rectangle _backgroundRectangle;
    private readonly Rectangle _selectionRectangle;
    private readonly Text _inputText;
    private readonly Rectangle _caretRectangle;

    private uint? _dragStartIndex;

    private bool _shiftPressed = false;
    private bool _ctrlPressed = false;

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
    /// The color of the caret line, pointing to the character to insert/delete.
    /// </summary>
    public UIProperty<EditBox, Color> CaretColor { get; }

    /// <summary>
    /// The text selection color.
    /// </summary>
    public UIProperty<EditBox, Color> SelectionColor { get; }

    /// <summary>
    /// The font of the edited text.
    /// </summary>
    public UIProperty<EditBox, Font> Font { get; }

    /// <summary>
    /// The input text vertical alignment.
    /// </summary>
    public UIProperty<EditBox, IVerticalAlignment> TextVerticalAlignment { get; }

    /// <summary>
    /// Determines the caret position by identifying the index of the character that will be inserted
    /// by pressing a key.
    /// If the index is 0, the caret is before the first character, or at the neutral position if <see cref="Text"/> is empty.
    /// </summary>
    /// <exception cref="ArgumentException">If <see cref="CaretIndex"/> is greater than the <see cref="Text"/> string size.</exception>
    /// <remarks>
    /// Given the text "ABCD", if <see cref="CaretIndex"/> is <c>2</c> the caret will be before the character with index 2,
    /// so the caret will be displayed as follows "AB&lt;caret&gt;CD".
    /// </remarks>
    public UIProperty<EditBox, uint> CaretIndex { get; }

    /// <summary>
    /// Determines the text selection number of characters. If 0, no selection will be displayed,
    /// otherwise the selection will be displayed and the caret will be hidden.
    /// The selection will start from the character where the caret is positioned (<see cref="CaretIndex"/>)
    /// and it will spawn <see cref="SelectionLength"/> characters towards the right (if length is positive)
    /// or towards the left (if length is negative).
    /// </summary>
    public UIProperty<EditBox, int> SelectionLength { get; }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        if (Root == null) return Size.Zero;
        var height = _inputText.ComputeTextMaximumHeight();
        var estimatedNumberOfCharacters = 10;
        return new Size(height * estimatedNumberOfCharacters, height);
    }

    /// <summary>
    /// Creates a new edit box to insert text.
    /// </summary>
    /// <param name="application">The application that contains this button.</param>
    /// <param name="text">The initial input text.</param>
    public EditBox(Application application, string text = "") : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Text = new UIProperty<EditBox, string>(this, text);
        Font = new UIProperty<EditBox, Font>(this, application.DefaultFont, BindingType.DestinationToSource);
        TextColor = CreateNewColorPropertyFor<EditBox>(UISharpColorNames.TextColor);
        DisabledTextColor = CreateNewColorPropertyFor<EditBox>(UISharpColorNames.TextColorDisabled);
        FocusColor = CreateNewColorPropertyFor<EditBox>(UISharpColorNames.BorderColor);
        DisabledBackgroundColor = CreateNewColorPropertyFor<EditBox>(UISharpColorNames.BorderColor);
        CaretColor = new UIProperty<EditBox, Color>(this, TextColor, BindingType.DestinationToSource);
        SelectionColor = CreateNewColorPropertyFor<EditBox>(UISharpColorNames.SecondaryColor);
        TextVerticalAlignment = new UIProperty<EditBox, IVerticalAlignment>(this, Alignment.Center);
        CaretIndex = new UIProperty<EditBox, uint>(this, 0);
        SelectionLength = new UIProperty<EditBox, int>(this, 0);
        BackgroundColor.BindTheme(UISharpColorNames.PrimaryBackground);

        Padding.SetAll(5.0f);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BorderColor.BindDestinationToSource(FocusColor);
        Canvas.Add(_backgroundRectangle);

        _selectionRectangle = new Rectangle(application);
        _selectionRectangle.BackgroundColor.BindDestinationToSource(SelectionColor);
        Canvas.Add(_selectionRectangle);

        _inputText = new Text(application);
        _inputText.Font.BindDestinationToSource(Font);
        _inputText.DisplayText.BindDestinationToSource(Text);
        _inputText.TextHorizontalAlignment.Value = Alignment.Left;
        _inputText.TextVerticalAlignment.BindDestinationToSource(TextVerticalAlignment);
        UpdateTextColorBinding();
        Canvas.Add(_inputText);

        _caretRectangle = new Rectangle(application);
        _caretRectangle.BackgroundColor.BindDestinationToSource(CaretColor);
        Canvas.Add(_caretRectangle);

        _textBuffer = new EditableTextBuffer(application);
        _textBuffer.Text.BindTwoWays(Text);
        _textBuffer.CaretIndex.BindTwoWays(CaretIndex);
        _textBuffer.SelectionLength.BindTwoWays(SelectionLength);
        _textBuffer.Text.Value = text;

        MousePress += EditBox_MousePress;
        KeyPress += EditBox_KeyPress;
        KeyRelease += EditBox_KeyRelease;
        TextInput += EditBox_TextInput;
        MouseDragBegin += EditBox_MouseDragBegin;
        MouseDrag += EditBox_MouseDrag;
        MouseEnter += EditBox_MouseEnter;
        MouseMove += EditBox_MouseEnter;
        MouseLeave += EditBox_MouseLeave;
        Enabled.ValueChange += Enabled_ValueChange;
        Focus.ValueChange += Focus_ValueChange;
        RenderFrame += EditBox_RenderFrame;
    }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();
        if (CaretIndex.Value > Text.Value.Length)
        {
            throw new IndexOutOfRangeException($"Invalid caret position {CaretIndex.Value} within text '{Text.Value}': caret position cannot exceed text length");
        }

        var selectionEnd = CaretIndex.Value + SelectionLength.Value;
        if (selectionEnd < 0 || selectionEnd > Text.Value.Length)
        {
            throw new IndexOutOfRangeException($"Invalid selection range for text '{Text.Value}' caused by invalid selection length {SelectionLength.Value}");
        }
    }

    private void EditBox_KeyPress(UIComponent sender, KeyEventArgs e)
    {
        var key = e.Button;

        // Need to make sure that selection and caret position are consistent with the string.
        Validate();

        if (key == KeyButton.LeftShift || key == KeyButton.RightShift)
        {
            _shiftPressed = true;
        }
        else if (key == KeyButton.LeftControl || key == KeyButton.RightControl)
        {
            _ctrlPressed = true;
        }
        else if (key == KeyButton.Backspace)
        {
            _textBuffer.DeleteLeft(_ctrlPressed);
        }
        else if (key == KeyButton.Delete)
        {
            _textBuffer.DeleteRight(_ctrlPressed);
        }
        else if (key == KeyButton.Right)
        {
            var offset = _ctrlPressed ? _textBuffer.GetNextTokenOffset() : 1;
            _textBuffer.MoveCaret(offset, _shiftPressed);
        }
        else if (key == KeyButton.Left)
        {
            var offset = _ctrlPressed ? _textBuffer.GetPreviousTokenOffset() : -1;
            _textBuffer.MoveCaret(offset, _shiftPressed);
        }
        else if (key == KeyButton.Up || key == KeyButton.Home)
        {
            _textBuffer.MoveCaret((int)-CaretIndex.Value, _shiftPressed);
        }
        else if (key == KeyButton.Down || key == KeyButton.End)
        {
            _textBuffer.MoveCaret(Text.Value.Length - (int)CaretIndex.Value, _shiftPressed);
        }
        else if (_ctrlPressed && key == KeyButton.A)
        {
            _textBuffer.SelectAll();
        }
        else if (_ctrlPressed && key == KeyButton.C && SelectionLength.Value != 0)
        {
            var (startIndex, endIndex) = _textBuffer.GetSelectionInterval();
            var selectionLength = (int)(endIndex - startIndex);
            Application.WindowManager.CopyToClipboard(Text.Value.Substring((int)startIndex, selectionLength));
        }
        else if (_ctrlPressed && key == KeyButton.V)
        {
            var textToPaste = Application.WindowManager.GetClipboardContent();
            _textBuffer.AddText(textToPaste);
        }
        else if (_ctrlPressed && key == KeyButton.X && SelectionLength.Value != 0)
        {
            var (startIndex, endIndex) = _textBuffer.GetSelectionInterval();
            var selectionLength = (int)(endIndex - startIndex);
            Application.WindowManager.CopyToClipboard(Text.Value.Substring((int)startIndex, selectionLength));
            _textBuffer.DeleteCurrentSelectedText();
        }
    }

    private void EditBox_KeyRelease(UIComponent sender, KeyEventArgs e)
    {
        var key = e.Button;
        if (key == KeyButton.LeftShift || key == KeyButton.RightShift)
        {
            _shiftPressed = false;
        }
        else if (key == KeyButton.LeftControl || key == KeyButton.RightControl)
        {
            _ctrlPressed = false;
        }
    }

    private void EditBox_TextInput(UIComponent sender, TextInputEventArgs e)
    {
        _textBuffer.AddText(e.Text);
    }

    private void EditBox_MouseDragBegin(UIComponent sender, MouseEventArgs e)
    {
        _dragStartIndex = ConvertPointToCharacterIndex(e.RelativeLocation);
    }

    private void EditBox_MouseDrag(UIComponent sender, MouseEventArgs e)
    {
        var caretIndex = ConvertPointToCharacterIndex(e.RelativeLocation);
        _textBuffer.SetSelectionDragInterval(_dragStartIndex!.Value, caretIndex);
    }

    private void EditBox_MouseEnter(UIComponent sender, EventArgs e)
    {
        Application.WindowManager.Cursor = MouseCursor.Edit;
    }

    private void EditBox_MouseLeave(UIComponent sender, EventArgs e)
    {
        Application.WindowManager.Cursor = MouseCursor.Default;
    }


    private void EditBox_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            Focus.Value = true;

            if (e.Clicks == 1)
            {
                CaretIndex.Value = ConvertPointToCharacterIndex(e.RelativeLocation);
                SelectionLength.Value = 0;
            }
            else if (e.Clicks == 2)
            {
                _textBuffer.SelectCurrentToken();
            }
            else if (e.Clicks == 3)
            {
                _textBuffer.SelectAll();
            }
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
            _inputText.TextColor.BindDestinationToSource(TextColor);
        }
        else
        {
            _inputText.TextColor.BindDestinationToSource(DisabledTextColor);
        }
    }

    private void EditBox_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        UpdateBackgroundRectangle(e.RenderingAreaSize);
        UpdateInputText(e.RenderingAreaSize);
        UpdateCaret(e.RenderingAreaSize);
        UpdateSelection(e.RenderingAreaSize);
    }

    private void UpdateBackgroundRectangle(Size renderingAreaSize)
    {
        _backgroundRectangle.RelativeRenderingArea.Value = renderingAreaSize.Fill();
        var cornerRadius = _backgroundRectangle.ComputeCornerRadius(0.5f, renderingAreaSize);
        _backgroundRectangle.CornerRadiusX.Value = cornerRadius;
        _backgroundRectangle.CornerRadiusY.Value = cornerRadius;
        UpdateBackgroundRectangleColor();
        UpdateBackgroundRectangleBorder();
    }

    private void UpdateInputText(Size renderingAreaSize)
    {
        var padding = Padding.ToMarginStruct();
        var textSize = _inputText.ComputeTextSize();
        var textX = _inputText.TextHorizontalAlignment.Value.ComputeX(renderingAreaSize, textSize);
        var textY = TextVerticalAlignment.Value.ComputeY(renderingAreaSize, textSize);
        _inputText.RelativeRenderingArea.Value = new Area(new Point(textX, textY), textSize).Clamp(renderingAreaSize, padding);
        _inputText.ClipArea.Value = _inputText.RelativeRenderingArea.Value;
    }

    private void UpdateCaret(Size renderingAreaSize)
    {
        _caretRectangle.Visible.Value = Focus.Value;

        var padding = Padding.ToMarginStruct();
        var textLeftBoundingBox = _inputText.RelativeRenderingArea.Value.TopLeft.X;
        var textRightBoundingBox = _inputText.RelativeRenderingArea.Value.BottomRight.X;

        var textStartPosition = textLeftBoundingBox + _inputText.HorizontalScroll.Value;
        var caretPosition = textStartPosition + _inputText.ComputeTextSize(CaretIndex.Value).Width;
        if (caretPosition < textLeftBoundingBox)
        {
            var delta = textLeftBoundingBox - caretPosition;
            _inputText.HorizontalScroll.Value += (delta);
            caretPosition += delta;
        }
        else if (caretPosition > textRightBoundingBox)
        {
            var delta = textRightBoundingBox - caretPosition;
            _inputText.HorizontalScroll.Value += delta;
            caretPosition += delta;
        }

        _caretRectangle.RelativeRenderingArea.Value =
            new Area(new Point(caretPosition, PixelUnit.Zero),
                     new Size(1, renderingAreaSize.Height)).Clamp(renderingAreaSize, padding);
    }

    private void UpdateSelection(Size renderingAreaSize)
    {
        _selectionRectangle.Visible.Value = SelectionLength.Value != 0;
        if (!_selectionRectangle.Visible.Value) return;

        var padding = Padding.ToMarginStruct();
        var textLeftBoundingBox = _inputText.RelativeRenderingArea.Value.TopLeft.X;
        var textStartPosition = textLeftBoundingBox + _inputText.HorizontalScroll.Value;
        var (startIndex, endIndex) = _textBuffer.GetSelectionInterval();
        var selectionStartPosition = textStartPosition + _inputText.ComputeTextSize(startIndex).Width;
        var selectionEndPosition = textStartPosition + _inputText.ComputeTextSize(endIndex).Width;

        _selectionRectangle.RelativeRenderingArea.Value =
            new Area(new Point(selectionStartPosition, PixelUnit.Zero),
                     new Size(selectionEndPosition - selectionStartPosition, renderingAreaSize.Height));
        _selectionRectangle.ClipArea.Value = renderingAreaSize.Fill().Clamp(renderingAreaSize, padding);
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
            _backgroundRectangle.BorderThickness.Value = 3.0f; // TODO: use styles
        }
        else
        {
            _backgroundRectangle.BorderThickness.Value = 0.0f;
        }
    }

    private uint ConvertPointToCharacterIndex(Point point)
    {
        var textStartPosition = GetTextStartPosition();
        var processedTextPosition = textStartPosition;
        var widths = _inputText.ComputeCharactersWidths(Text.Value, Font.Value);
        var processedTextIndex = 0u;
        foreach (var width in widths)
        {
            if (processedTextPosition + width / 2.0f >= point.X)
            {
                return processedTextIndex;
            }

            processedTextPosition += width;
            processedTextIndex++;
        }

        return processedTextIndex;
    }

    private PixelUnit GetTextStartPosition()
    {
        var textLeftBoundingBox = _inputText.RelativeRenderingArea.Value.TopLeft.X;
        return textLeftBoundingBox + _inputText.HorizontalScroll.Value;
    }
}
