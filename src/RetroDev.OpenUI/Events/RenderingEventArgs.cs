using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Events;

public class RenderingEventArgs : EventArgs
{
    public Canvas Canvas { get; }

    public RenderingEventArgs(Canvas canvas)
    {
        Canvas = canvas;
    }

    /// <inheritdoc />
    public override string ToString() => string.Empty;
}
