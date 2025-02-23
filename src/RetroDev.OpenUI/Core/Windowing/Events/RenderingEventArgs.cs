using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing.Events;

public class RenderingEventArgs : EventArgs
{
    /// <summary>
    /// The size of the area where to render a component.
    /// </summary>
    public Size RenderingAreaSize { get; }

    public RenderingEventArgs(Size renderingAreaSize)
    {
        RenderingAreaSize = renderingAreaSize;
    }

    /// <inheritdoc />
    public override string ToString() => $"{RenderingAreaSize}";
}
