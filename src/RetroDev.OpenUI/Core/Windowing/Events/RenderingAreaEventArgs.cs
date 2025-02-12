using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing.Events;

public class RenderingAreaEventArgs(Area renderingArea) : EventArgs
{
    public Area RenderingArea { get; } = renderingArea;

    /// <inheritdoc />
    public override string ToString() => RenderingArea.ToString();
}
