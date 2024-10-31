using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

/// <summary>
/// Represents a 2D-point.
/// </summary>
/// <param name="X">The X location in pixels.</param>
/// <param name="Y">The Y location in pixels.</param>
[DebuggerDisplay("({X}, {Y})")]
public record Point(CoordinateType X, CoordinateType Y)
{
    public static readonly Point ZERO = new(0, 0);

    /// <summary>
    /// Whether <see langword="this"/> point is within the given <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The area to check against.</param>
    /// <returns><see langword="true"/> if the point is within the given area, otehrwise <see langword="false"/>.</returns>
    public bool IsWithin(Area area) =>
        (X >= area.TopLeft.X && X <= area.TopLeft.X + area.Size.Width) &&
        (Y >= area.TopLeft.Y && Y <= area.TopLeft.Y + area.Size.Height);

    public static Point operator +(Point p1, Point p2) => new(p1.X + p2.X, p1.Y + p2.Y);
    public static Point operator -(Point p1, Point p2) => new(p1.X - p2.X, p1.Y - p2.Y);
}
