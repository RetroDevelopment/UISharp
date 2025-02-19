using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Graphics.Coordinates;

/// <summary>
/// Represents a 2D-point.
/// </summary>
/// <param name="X">The X location in pixels.</param>
/// <param name="Y">The Y location in pixels.</param>
[DebuggerDisplay("({X}, {Y})")]
public record Point(PixelUnit X, PixelUnit Y)
{
    public static readonly Point Zero = new(0, 0);

    /// <summary>
    /// Creates a new <see cref="Point"/> with both <see cref="X"/> and <see cref="Y"/> are set to <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The point <see cref="X"/> and <see cref="Y"/> values.</param>
    public Point(PixelUnit value) : this(value, value) { }

    /// <summary>
    /// Whether <see langword="this"/> point is within the given <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The area to check against.</param>
    /// <returns><see langword="true"/> if the point is within the given area, otehrwise <see langword="false"/>.</returns>
    public bool IsWithin(Area area) =>
        X >= area.TopLeft.X && X <= area.TopLeft.X + area.Size.Width &&
        Y >= area.TopLeft.Y && Y <= area.TopLeft.Y + area.Size.Height;

    /// <summary>
    /// Makes sure <see langword="this" /> point is contained within <paramref name="topLeft"/> and <paramref name="bottomRight"/>.
    /// </summary>
    /// <param name="topLeft">The minimum <see cref="X"/> and <see cref="Y"/> values allowed for the returned point.</param>
    /// <param name="bottomRight">The maximum <see cref="X"/> and <see cref="Y"/> values allowed for the returned point.</param>
    /// <returns>
    /// A <see cref="Point"/> wihin the rectangular area marked by <paramref name="topLeft"/> and <paramref name="bottomRight"/>.
    /// </returns>
    /// <remarks>
    /// If any input has value <see cref="PixelUnit.Auto"/> it is undefined behavior.
    /// </remarks>
    public Point Clamp(Point topLeft, Point bottomRight) =>
        new(Math.Clamp(X, topLeft.X, bottomRight.X), Math.Clamp(Y, topLeft.Y, bottomRight.Y));

    /// <summary>
    /// Converts <see langword="this" /> coordinate (assuming it is a screen coordinate) into
    /// a cartesian coordinate whose origin is in the center of the given <paramref name="area"/>.
    /// </summary>
    /// <param name="areaSize">The size of the area whose cartesian coordinate origin lies.</param>
    /// <returns>The cartesian point.</returns>
    public Point FromScreenToCartesian(Size areaSize)
    {
        var area = new Area(Zero, areaSize);
        var center = area.Center;

        return new(X - center.X, center.Y - Y);
    }

    public static Point operator +(Point p1, Point p2) => new(p1.X + p2.X, p1.Y + p2.Y);
    public static Point operator -(Point p1, Point p2) => new(p1.X - p2.X, p1.Y - p2.Y);

    /// <inheritdoc />
    public override string ToString() =>
        $"({X.Value}, {Y.Value})";
}
