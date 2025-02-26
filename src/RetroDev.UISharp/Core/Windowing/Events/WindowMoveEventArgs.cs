using RetroDev.UISharp.Core.Graphics.Coordinates;

namespace RetroDev.UISharp.Core.Windowing.Events;

public class WindowMoveEventArgs(Point topLeft) : EventArgs
{
    public Point TopLeft { get; } = topLeft;

    public override string ToString() =>
        TopLeft.ToString();
}
