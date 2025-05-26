using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Core.Windowing.Events;

public class WindowMoveEventArgs(Point topLeft) : EventArgs
{
    public Point TopLeft { get; } = topLeft;

    public override string ToString() =>
        TopLeft.ToString();
}
