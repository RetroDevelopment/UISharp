using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

/// <summary>
/// Represents a 2D area size.
/// </summary>
/// <param name="Width">The width in pixels.</param>
/// <param name="Height">The height in pixels.</param>
[DebuggerDisplay("{Width} x {Height}")]
public record Size(CoordinateType Width, CoordinateType Height)
{
    public static readonly Size ZERO = new(0, 0);
}
