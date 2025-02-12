using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.UI;

namespace RetroDev.OpenUI.Core.Windowing.Events;

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
