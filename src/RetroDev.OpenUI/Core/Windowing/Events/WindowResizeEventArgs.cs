using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing.Events;

public class WindowResizeEventArgs(Size size) : EventArgs
{
    public Size Size { get; } = size;

    public override string ToString() =>
        Size.ToString();
}
