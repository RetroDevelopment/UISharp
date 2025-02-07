using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Events;

public class RenderingAreaEventArgs(Area renderingArea) : EventArgs
{
    public Area RenderingArea { get; } = renderingArea;

    /// <inheritdoc />
    public override string ToString() => RenderingArea.ToString();
}
