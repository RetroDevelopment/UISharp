using RetroDev.UISharp.Core.Graphics.Coordinates;

namespace RetroDev.UISharp.Core.Windowing.Events;

public class RenderingAreaEventArgs(Area renderingArea) : EventArgs
{
    public Area RenderingArea { get; } = renderingArea;

    /// <inheritdoc />
    public override string ToString() => RenderingArea.ToString();
}
