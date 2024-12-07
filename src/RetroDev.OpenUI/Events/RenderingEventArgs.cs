using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Events;

public class RenderingEventArgs : EventArgs
{
    public Canvas Canvas { get; }

    public RenderingEventArgs(Canvas canvas)
    {
        Canvas = canvas;
    }
}
