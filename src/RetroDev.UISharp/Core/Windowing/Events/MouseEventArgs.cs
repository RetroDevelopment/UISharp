using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Core.Windowing.Events;

/// <summary>
/// Argument for mouse events.
/// </summary>
/// <param name="absoluteLocation">The mouse location when the event appears relative to the root container (e.g. window).</param>
/// <param name="relativeLocation">The mouse location when the event appears relative to the component where the event occurs.</param>
/// <param name="button">The mouse button pressed, if any.</param>
public class MouseEventArgs(Point absoluteLocation, Point relativeLocation, MouseButton button) : EventArgs
{
    /// <summary>
    /// The mouse location when the event appears. The location is in pixels and it is the location
    /// relative to the root container, usually the window where the mouse movement occur.
    /// </summary>
    public Point AbsoluteLocation { get; } = absoluteLocation;


    /// <summary>
    /// The mouse location when the event appears. The location is in pixels and it is relative to the
    /// parent container. For example, if the mouse move is within a button, the location is not relative to the
    /// window containig the button, but to the button area itself, so (0,0) means button top-left corner.
    /// </summary>
    public Point RelativeLocation { get; } = relativeLocation;

    /// <summary>
    /// The mouse button pressed, if any.
    /// </summary>
    public MouseButton Button { get; } = button;

    /// <inheritdoc />
    public override string ToString() => $"locationAbs={AbsoluteLocation}, locationRel={RelativeLocation}, button={Button}";
}
