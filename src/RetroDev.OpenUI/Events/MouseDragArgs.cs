using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Events;

/// <summary>
/// Argument for mouse drag events.
/// </summary>
/// <param name="clickPointAbsolute">Absolute location of the point where the mouse was clicked.</param>
/// <param name="lastClickPointAbsolute">Absolute location of the point where the mouse is at now.</param>
/// <param name="offset">Movement since last mouse location was captured.</param>
public class MouseDragArgs(Point clickPointAbsolute, Point lastClickPointAbsolute, Point offset) : EventArgs
{
    /// <summary>
    /// Absolute location of the point where the mouse was clicked.
    /// </summary>
    public Point ClickPointAbsolute { get; } = clickPointAbsolute;

    /// <summary>
    /// Absolute location of the point where the mouse is at now.
    /// </summary>
    public Point LastClickPointAbsolute { get; } = lastClickPointAbsolute;

    /// <summary>
    /// Movement since last mouse location was captured.
    /// </summary>
    public Point Offset { get; } = offset;
}
