﻿using System.Text.RegularExpressions;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core;

using Token = (uint Start, uint End);

/// <summary>
/// Contains all the utilities to manage editable text logic.
/// </summary>
public class EditableTextBuffer
{
    private readonly List<Token> _tokens = [];
    private readonly List<string> _history = [];

    private int _historyIndex;

    /// <summary>
    /// The regex to detect token delimiters.
    /// The <see cref="Text"/> is split into tokens (by this <see cref="TokenRegex"/>) for
    /// rapid movements and deletes (e.g. with CTRL + delete).
    /// </summary>
    public string TokenRegex { get; set; } = @"\S+";

    /// <summary>
    /// The buffer text.
    /// </summary>
    public UIProperty<string> Text { get; }

    /// <summary>
    /// Determines the caret position by identifying the index of the character that will be inserted
    /// by pressing a key.
    /// If the index is 0, the caret is before the first character, or at the neutral position if <see cref="Text"/> is empty.
    /// </summary>
    public UIProperty<uint> CaretIndex { get; }

    /// <summary>
    /// Determines the text selection number of characters. If 0, no selection will be displayed,
    /// otherwise the selection will be displayed and the caret will be hidden.
    /// The selection will start from the character where the caret is positioned (<see cref="CaretIndex"/>)
    /// and it will spawn <see cref="SelectionLength"/> characters towards the right (if length is positive)
    /// or towards the left (if length is negative).
    /// </summary>
    public UIProperty<int> SelectionLength { get; }

    /// <summary>
    /// Creates a new instance of <see cref="EditableTextBuffer"/>.
    /// </summary>
    /// <param name="application">The application owning this buffer.</param>
    public EditableTextBuffer(Application application)
    {
        Text = new UIProperty<string>(application, string.Empty);
        CaretIndex = new UIProperty<uint>(application, 0);
        SelectionLength = new UIProperty<int>(application, 0);

        Text.ValueChange.Subscribe(OnTextValueChange);
        _history.Add(Text.Value);
        _historyIndex = 0;
    }

    /// <summary>
    /// Deletes the character at the left of the caret (if any) or the portion of text
    /// marked by the selection.
    /// </summary>
    /// <param name="deleteToken">
    /// Whether to delete the full part of the current token at the left of the caret or just the 
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
    /// Whether to delete the full part of the current token at the right of the caret or just the 
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
    /// Positive offsets move the caret towards the right, negative offsets move it toward the left.
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
            caretIndex = offset < 0 ? selectionInterval.StartIndex : selectionInterval.EndIndex;
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
    /// Reverts the <see cref="Text"/> value to the value it had the last time <see cref="TakeHistorySnapshot"/> was called.
    /// </summary>
    public void Undo()
    {
        if (_historyIndex == 0) return;
        _historyIndex--;
        Text.Value = _history[_historyIndex];
        CaretIndex.Value = (uint)Text.Value.Length;
        SelectionLength.Value = 0;
    }

    /// <summary>
    /// Reverts the <see cref="Text"/> value to the value it had the last time <see cref="Undo"/> was performed.
    /// </summary>
    public void Redo()
    {
        if (_historyIndex >= _history.Count - 1) return;
        _historyIndex++;
        Text.Value = _history[_historyIndex];
        CaretIndex.Value = (uint)Text.Value.Length;
        SelectionLength.Value = 0;
    }

    /// <summary>
    /// Stores the current <see cref="Text"/> value in the internal history used by <see cref="Undo"/> and <see cref="Redo"/>.
    /// operations.
    /// </summary>
    /// <param name="text">The text for which to take the snapshot.</param>
    /// <remarks>Duplicate strings are disregarded, meaning that if two snapshots are taken with the same <see cref="Text"/>, the second one will be disregarded to avoid for example multiple equal undo operations.</remarks>
    public virtual void TakeHistorySnapshot(string text)
    {
        if (_history.Count > 0 && _history[_historyIndex] == text) return;

        if (_historyIndex < _history.Count - 1)
        {
            _history.RemoveRange(_historyIndex + 1, _history.Count - _historyIndex - 1);
        }

        _history.Add(text);
        _historyIndex = _history.Count - 1;
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
    /// <param name="multiTokenSelection">
    /// if <see langword="true" /> it includes the full <paramref name="dragStartIndex"/> and <paramref name="caretIndex"/>
    /// tokens in the selection.
    /// </param>
    public virtual void SetSelectionDragInterval(uint dragStartIndex, uint caretIndex, bool multiTokenSelection)
    {
        if (multiTokenSelection)
        {
            var caretIndexToken = GetTokenAt(caretIndex);
            var dragStartToken = GetTokenAt(dragStartIndex);
            CaretIndex.Value = caretIndex < dragStartIndex ? caretIndexToken.Start : caretIndexToken.End + 1;
            var selectionStart = (int)(caretIndex < dragStartIndex ? dragStartToken.End + 1 : dragStartToken.Start);
            SelectionLength.Value = (int)(selectionStart - CaretIndex.Value);
        }
        else
        {
            CaretIndex.Value = caretIndex;
            SelectionLength.Value = (int)(dragStartIndex - caretIndex);
        }
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
        var caretIndex = CaretIndex.Value;
        if (caretIndex >= Text.Value.Length) return 0;

        var nextTokenIndex = _tokens.FindIndex(interval => interval.Start > caretIndex);
        if (nextTokenIndex < 0) return Text.Value.Length - (int)caretIndex;
        return (int)_tokens[nextTokenIndex].Start - (int)caretIndex;
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
        var caretIndex = CaretIndex.Value;
        if (caretIndex == 0) return 0;

        var previousTokenIndex = _tokens.FindLastIndex(interval => interval.Start < caretIndex);
        if (previousTokenIndex < 0) return -(int)caretIndex;
        return (int)_tokens[previousTokenIndex].Start - (int)caretIndex;
    }

    /// <summary>
    /// Selects the full token.
    /// </summary>
    public virtual void SelectCurrentToken()
    {
        if (Text.Value.Length == 0) return;
        var caretIndex = CaretIndex.Value;
        var interval = GetTokenAt(caretIndex);
        CaretIndex.Value = interval.Start;
        SelectionLength.Value = (int)((interval.End + 1) - interval.Start);
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
    /// The pair <c>(StartIndex, EndIndex)</c> identifying the indexes of the first and last selected
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

    /// <summary>
    /// Gets the token where the character at the given <paramref name="index"/> lies.
    /// </summary>
    /// <param name="index">The index of the character for which to find the token.</param>
    /// <returns>The token start and end 0-based index.</returns>
    public virtual Token GetTokenAt(uint index) =>
        _tokens.FindLast(interval => index >= interval.Start);

    private void RefreshTokenList()
    {
        _tokens.Clear();
        var text = Text.Value;
        var match = Regex.Match(text, TokenRegex);
        while (match.Success)
        {
            _tokens.Add(((uint)match.Index, (uint)(match.Index + match.Length - 1)));
            match = match.NextMatch();
        }
    }

    private void OnTextValueChange(string text)
    {
        CaretIndex.Value = (uint)Math.Clamp(CaretIndex.Value, 0, text.Length);
        SelectionLength.Value = Math.Clamp(SelectionLength.Value, -(int)CaretIndex.Value, text.Length - (int)CaretIndex.Value);
        RefreshTokenList();
    }
}
