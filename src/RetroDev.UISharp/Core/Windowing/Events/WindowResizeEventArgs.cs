using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Core.Windowing.Events;

public class WindowResizeEventArgs(Size size) : EventArgs
{
    public Size Size { get; } = size;

    public override string ToString() =>
        Size.ToString();
}
