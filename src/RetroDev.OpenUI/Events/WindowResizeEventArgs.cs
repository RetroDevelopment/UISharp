using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Events;

public class WindowResizeEventArgs(Size size) : EventArgs
{
    public Size Size { get; } = size;

    public override string ToString() =>
        Size.ToString();
}
