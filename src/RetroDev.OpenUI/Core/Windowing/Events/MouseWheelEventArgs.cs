using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing.Events;

/// <summary>
/// Mouse wheel movement.
/// </summary>
/// <param name="horizontalMovement">Mouse wheel x movement in pixels.</param>
/// <param name="verticalMovement">Mouse wheel y movement in pixels.</param>
/// <param name="absoluteLocation">The mouse location when the event appears relative to the root container (e.g. window).</param>
/// <param name="relativeLocation">The mouse location when the event appears relative to the component where the event occurs.</param>
public class MouseWheelEventArgs(PixelUnit horizontalMovement, PixelUnit verticalMovement, Point absoluteLocation, Point relativeLocation) : EventArgs
{
    /// <summary>
    /// Mouse wheel x movement in pixels.
    /// </summary>
    public PixelUnit HorizontalMovement { get; } = horizontalMovement;

    /// <summary>
    /// Mouse wheel y movement in pixels.
    /// </summary>
    public PixelUnit VerticalMovement { get; } = verticalMovement;

    /// <summary>
    /// The mouse location when the event appears relative to the root container (e.g. window).
    /// </summary>
    public Point AbsoluteLocation { get; } = absoluteLocation;

    /// <summary>
    /// The mouse location when the event appears relative to the component where the event occurs.
    /// </summary>
    public Point RelativeLocation { get; } = relativeLocation;

    public override string ToString() =>
        $"horizontal: {HorizontalMovement}, vertical: {VerticalMovement}";
}
