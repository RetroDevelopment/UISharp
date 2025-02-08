namespace RetroDev.OpenUI.Core.Windowing.Events;

/// <summary>
/// Argument for text input event.
/// </summary>
/// <param name="text">The text inserted from keyboard.</param>
public class TextInputEventArgs(string text) : EventArgs
{
    /// <summary>
    /// The text inserted from keyboard.
    /// </summary>
    public string Text { get; } = text;

    /// <inheritdoc />
    public override string ToString() => Text;
}
