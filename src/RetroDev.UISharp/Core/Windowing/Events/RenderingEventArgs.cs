using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Graphics.Coordinates;

namespace RetroDev.UISharp.Core.Windowing.Events;

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
