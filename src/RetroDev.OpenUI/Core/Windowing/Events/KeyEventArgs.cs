namespace RetroDev.OpenUI.Core.Windowing.Events;

/// <summary>
/// Argument for keyboard key events.
/// </summary>
/// <param name="button">The button pressed or release.</param>
public class KeyEventArgs(KeyButton button) : EventArgs
{
    /// <summary>
    /// The button pressed or release.
    /// </summary>
    public KeyButton Button { get; } = button;

    /// <inheritdoc />
    public override string ToString() => $"key={Button}";
}
