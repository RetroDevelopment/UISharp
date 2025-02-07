using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Events;

public class WindowMoveEventArgs(Point topLeft) : EventArgs
{
    public Point TopLeft { get; } = topLeft;

    public override string ToString() =>
        TopLeft.ToString();
}
