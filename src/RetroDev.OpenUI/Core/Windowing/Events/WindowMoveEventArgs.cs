using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing.Events;

public class WindowMoveEventArgs(Point topLeft) : EventArgs
{
    public Point TopLeft { get; } = topLeft;

    public override string ToString() =>
        TopLeft.ToString();
}
