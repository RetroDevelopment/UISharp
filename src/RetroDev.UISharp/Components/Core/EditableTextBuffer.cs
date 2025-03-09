using System.Text.RegularExpressions;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core;

/// <summary>
/// Contains all the utilities to manage editable text logic.
/// </summary>
public class EditableTextBuffer
{
    /// <summary>
    /// The regex to detect token delimiters.
    /// The <see cref="Text"/> is split into tokens (by this <see cref="TokenRegex"/>) for
    /// rapid movements and deletes (e.g. with CTRL + delete).
    /// </summary>
    public string TokenRegex { get; set; } = @"(?<=\s)(\S)|(?<=\W)(\w)";

    /// <summary>
    /// The buffer text.
    /// </summary>
    public BindableProperty<string> Text { get; }

    /// <summary>
    /// Determines the caret position by identifying the index of the character that will be inserted
    /// by pressing a key.
    /// If the index is 0, the caret is before the first character, or at the neutral position if <see cref="Text"/> is empty.
    /// </summary>
    public BindableProperty<uint> CaretIndex { get; }

    /// <summary>
    /// Determines the text selection number of characters. If 0, no selection will be displayed,
    /// otherwise the selection will be displayed and the caret will be hidden.
    /// The selection will start from the character where the caret is positioned (<see cref="CaretIndex"/>)
    /// and it will spawn <see cref="SelectionLength"/> characters towards the right (if length is positive)
    /// or towards the left (if length is negative).
    /// </summary>
    public BindableProperty<int> SelectionLength { get; }

    /// <summary>
    /// Creates a new instance of <see cref="EditableTextBuffer"/>.
    /// </summary>
    /// <param name="application">The application owning this buffer.</param>
    public EditableTextBuffer(Application application)
    {
        Text = new BindableProperty<string>(string.Empty, application);
        CaretIndex = new BindableProperty<uint>(0, application);
        SelectionLength = new BindableProperty<int>(0, application);

        Text.ValueChange += Text_ValueChange;
    }

    /// <summary>
    /// Deletes the character at the left of the caret (if any) or the portion of text
    /// marked by the selection.
    /// </summary>
    /// <param name="deleteToken">
    /// Whether to thelete the full part of the current token at the left of the caret or just the 
    /// character at the left of the caret.
    /// </param>
    public virtual void DeleteLeft(bool deleteToken)
    {
        var caretIndex = CaretIndex.Value;
        if (SelectionLength.Value == 0 && caretIndex > 0)
        {
            var interval = deleteToken ? GetPreviousTokenOffset() : -1;
            Text.Value = Text.Value.Remove((int)caretIndex + interval, -interval);
            CaretIndex.Value = (uint)(caretIndex + interval);
        }
        else
        {
            DeleteCurrentSelectedText();
        }
    }

    /// <summary>
    /// Deletes the character at the right of the caret (if any) or the portion of text
    /// marked by the selection.
    /// </summary>
    /// <param name="deleteToken">
    /// Whether to thelete the full part of the current token at the right of the caret or just the 
    /// character at the right of the caret.
    /// </param>
    public virtual void DeleteRight(bool deleteToken)
    {
        var caretIndex = CaretIndex.Value;
        var selectionLength = SelectionLength.Value;

        if (selectionLength == 0 && caretIndex < Text.Value.Length)
        {
            var interval = deleteToken ? GetNextTokenOffset() : 1;
            Text.Value = Text.Value.Remove((int)caretIndex, interval);
        }
        else
        {
            DeleteCurrentSelectedText();
        }
    }

    /// <summary>
    /// Moves the caret by the number of characters specified by <paramref name="offset"/>, or
    /// moves the selection range if <paramref name="selectionActive"/> is <see langword="true" />.
    /// </summary>
    /// <param name="offset">
    /// The number of character for which to move the caret.
    /// Positive offsets move the caret towards the right, netative offsets move it torwards the left.
    /// </param>
    /// <param name="selectionActive"></param>
    public virtual void MoveCaret(int offset, bool selectionActive)
    {
        if (Text.Value.Length == 0) return;

        var caretIndex = CaretIndex.Value;
        var selectionLegnth = SelectionLength.Value;

        var clampedOffset = (int)Math.Clamp(offset, -caretIndex, Text.Value.Length - CaretIndex.Value);
        caretIndex = (uint)(caretIndex + clampedOffset);

        if (selectionActive)
        {
            selectionLegnth -= clampedOffset;
        }
        else if (SelectionLength.Value != 0)
        {
            selectionLegnth = 0;
            var selectionInterval = GetSelectionInterval();
            caretIndex = clampedOffset < 0 ? selectionInterval.StartIndex : selectionInterval.EndIndex;
        }

        CaretIndex.Value = caretIndex;
        SelectionLength.Value = selectionLegnth;
    }

    /// <summary>
    /// Adds the given <paramref name="character"/> to <see langword="this" /> <see cref="EditableTextBuffer"/>
    /// at the current caret position and updates the caret and selection position accordingly.
    /// </summary>
    /// <param name="text">The text to add.</param>
    public virtual void AddText(string text)
    {
        if (SelectionLength.Value != 0) DeleteCurrentSelectedText();
        var caretIndex = CaretIndex.Value;
        Text.Value = Text.Value.Insert((int)caretIndex, text);
        CaretIndex.Value = caretIndex + (uint)text.Length;
    }

    /// <summary>
    /// Sets the selection interval (useful for mouse dragging selection).
    /// </summary>
    /// <param name="dragStartIndex">The index of the character where the selection ends.
    /// For mouse drag, this is where the mouse drag started and it is also the fixed.
    /// </param>
    /// <param name="caretIndex">
    /// The index of the character where the selection starts and where the caret is.
    /// For mouse drag, this is the current mouse position.
    /// </param>
    public virtual void SetSelectionDragInterval(uint dragStartIndex, uint caretIndex)
    {
        CaretIndex.Value = caretIndex;
        SelectionLength.Value = (int)dragStartIndex - (int)caretIndex;
    }

    /// <summary>
    /// Gets the index of the next token (to the right of <see cref="CaretIndex"/>).
    /// A token is a unit of text for which the caret can be moved (e.g. when pressing ctrl).
    /// </summary>
    /// <returns>
    /// The index of the first character of next token to the right of <see cref="CaretIndex"/> or
    /// 1 if no other token exists from <see cref="CaretIndex"/>.
    /// </returns>
    public virtual int GetNextTokenOffset()
    {
        if (CaretIndex.Value >= Text.Value.Length) return 0;

        var rightCaretSubstring = Text.Value.Substring((int)CaretIndex.Value);
        var match = Regex.Match(rightCaretSubstring, TokenRegex, RegexOptions.None);

        var maxStep = (int)(Text.Value.Length - CaretIndex.Value);
        if (match.Success)
        {
            return Math.Clamp(match.Index, 0, maxStep);
        }

        return maxStep;
    }

    /// <summary>
    /// Gets the offset to reach the previous token (to the right of <see cref="CaretIndex"/>).
    /// A token is a unit of text for which the caret can be moved (e.g. when pressing ctrl).
    /// </summary>
    /// <returns>
    /// The index of the last character of previous token to the right of <see cref="CaretIndex"/> or
    /// -1 if <see cref="CaretIndex"/> is already pointing to the first token of the string.
    /// </returns>
    public virtual int GetPreviousTokenOffset()
    {
        if (CaretIndex.Value == 0) return 0;

        var leftCaretSubstring = Text.Value.Substring(0, (int)CaretIndex.Value);
        var match = Regex.Match(leftCaretSubstring, TokenRegex, RegexOptions.RightToLeft);

        var minStep = (int)(-CaretIndex.Value);
        if (match.Success)
        {
            return Math.Clamp((int)(match.Index - CaretIndex.Value), minStep, 0);
        }

        return minStep;
    }

    /// <summary>
    /// Selects the full token.
    /// </summary>
    public virtual void SelectCurrentToken()
    {
        var start = CaretIndex.Value + GetPreviousTokenOffset();
        var end = CaretIndex.Value + GetNextTokenOffset();
        CaretIndex.Value = (uint)start;
        SelectionLength.Value = (int)(end - start);
    }

    /// <summary>
    /// Selects the full text.
    /// </summary>
    public virtual void SelectAll()
    {
        CaretIndex.Value = 0;
        SelectionLength.Value = Text.Value.Length;
    }

    /// <summary>
    /// Gets the text selection interval.
    /// </summary>
    /// <returns>
    /// The pair <c>(StartIndex, EndIndex)</c> identifying the idices of the first and last selected
    /// characters (extremes included).
    /// </returns>
    public virtual (uint StartIndex, uint EndIndex) GetSelectionInterval()
    {
        var caretEnd = CaretIndex.Value + SelectionLength.Value;
        var selectionStartIndex = (uint)Math.Min(CaretIndex.Value, caretEnd);
        var selectionEndIndex = (uint)Math.Max(CaretIndex.Value, caretEnd);
        return (selectionStartIndex, selectionEndIndex);
    }

    /// <summary>
    /// Deletes the current selection, if any.
    /// </summary>
    public virtual void DeleteCurrentSelectedText()
    {
        if (SelectionLength.Value == 0) return;

        var (startIndex, endIndex) = GetSelectionInterval();
        var removalLength = (int)(endIndex - startIndex);
        Text.Value = Text.Value.Remove((int)startIndex, removalLength);
        CaretIndex.Value = startIndex;
        SelectionLength.Value = 0;
    }

    private void Text_ValueChange(BindableProperty<string> sender, ValueChangeEventArgs<string> e)
    {
        CaretIndex.Value = (uint)Math.Clamp(CaretIndex.Value, 0, Text.Value.Length + 1);
    }
}
