using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing.Events;

/// <summary>
/// Mouse wheel movement.
/// </summary>
/// <param name="horizontalMovement">Mouse wheel x movement in pixels.</param>
/// <param name="verticalMovement">Mouse wheel y movement in pixels.</param>
public class MouseWheelEventArgs(PixelUnit horizontalMovement, PixelUnit verticalMovement) : EventArgs
{
    /// <summary>
    /// Mouse wheel x movement in pixels.
    /// </summary>
    public PixelUnit HorizontalMovement { get; } = horizontalMovement;

    /// <summary>
    /// Mouse wheel y movement in pixels.
    /// </summary>
    public PixelUnit VerticalMovement { get; } = verticalMovement;

    public override string ToString() =>
        $"horizontal: {HorizontalMovement}, vertical: {VerticalMovement}";
}
