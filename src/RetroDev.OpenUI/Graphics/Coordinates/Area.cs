using System.Diagnostics;
using System.Numerics;

namespace RetroDev.OpenUI.Core.Coordinates;

/// <summary>
/// Represents a 2D-Area.
/// </summary>
/// <param name="TopLeft">The area top-left corner coordinate in pixels.</param>
/// <param name="Size">The area size in pixels.</param>
[DebuggerDisplay("{TopLeft} ; {Size}")]
public record Area(Point TopLeft, Size Size)
{
    private readonly Point _center = new(TopLeft.X + Size.Width / 2, TopLeft.Y + Size.Height / 2);
    private readonly Point _bottomRight = new(TopLeft.X + Size.Width, TopLeft.Y + Size.Height);
    private readonly Point _bottomLeft = new(TopLeft.X, TopLeft.Y + Size.Height);

    /// <summary>
    /// The empty area which requires 0 pizels.
    /// </summary>
    public static readonly Area Empty = new(Point.Zero, Point.Zero);

    /// <summary>
    /// The area central point coordinate in pixels.
    /// </summary>
    public Point Center => _center;

    /// <summary>
    /// The area bottom-right point coordinate in pixels.
    /// </summary>
    public Point BottomRight => _bottomRight;

    /// <summary>
    /// The area bottom-left point coordinate in pixels.
    /// </summary>
    public Point BottomLeft => _bottomLeft;

    public Area(Point topLeft, Point bottomRight) : this(topLeft, new Size(bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y)) { }

    /// <summary>
    /// Transforms <see langword="this"/> area into an absolute area relative to the given <paramref name="container"/>.
    /// This means that if <see langword="this"/> area <see cref="TopLeft"/> coordinate is (0, 0), and the given <paramref name="container"/> area
    /// coordinate is (100, 100), the result is (100, 100).
    /// </summary>
    /// <param name="container">The area containing this area.</param>
    /// <returns>The absolute area realtive to the given <paramref name="container"/>.</returns>
    public Area ToAbsolute(Area? container) =>
        (container != null ? new(container.TopLeft + TopLeft, Size) : this);

    /// <summary>
    /// Clips this area so that it fits in the given <paramref name="container"/> area.
    /// </summary>
    /// <param name="container">The area containing <see langword="this"/> area.</param>
    /// <returns>The clipped area.</returns>
    public Area Clip(Area? container)
    {
        if (container == null) return this;

        var clippedTopLeft = TopLeft.Clamp(container.TopLeft, container.BottomRight);
        var clippedBottomRight = BottomRight.Clamp(container.TopLeft, container.BottomRight);

        return new Area(clippedTopLeft, clippedBottomRight);
    }

    /// <summary>
    /// Gets the top-left point that <see langword="this"/> area should have if it must be centered in the given
    /// <paramref name="container"/> area.
    /// </summary>
    /// <param name="container">The container area to center.</param>
    /// <returns>The top-left point to center <see langword="this"/> area.</returns>
    public Point CenterTopLeft(Area container)
    {
        var containerCenterX = container.TopLeft.X + container.Size.Width / 2;
        var containerCenterY = container.TopLeft.Y + container.Size.Height / 2;

        return new(containerCenterX - Size.Width / 2, containerCenterY - Size.Height / 2);
    }

    /// <summary>
    /// Creates a <see cref="Area"/> relative to <see langword="this" /> area that covers it completely.
    /// </summary>
    /// <remarks>
    /// This method is very useful when creating a drawing area relative to a container drawing area that fills it completely.
    /// For example, given a relative drawing area <c>a</c> with location (100, 100) with size 300 x 300, <c>a.Fill()</c> will return
    /// the area with location (0, 0) and size 300 x 300, which is the area relative to <c>a</c> that fully fills <c>a</c>.
    /// </remarks>
    /// <returns>The area with <see cref="Point.Zero"/> coordinate and the same <see cref="Size"/> as <see langword="this" /> area.</returns>
    public Area Fill() =>
        new Area(Point.Zero, Size);

    /// <summary>
    /// Merges <see langword="this" /> area with the given <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The <see cref="Area"/> to merge.</param>
    /// <returns>The minimum area that contains both <see langword="this" /> area and the given <paramref name="area"/>.</returns>
    public Area Merge(Area area)
    {
        var leftX = Math.Min(this.TopLeft.X, area.TopLeft.X);
        var topY = Math.Min(this.TopLeft.Y, area.TopLeft.Y);
        var rightX = Math.Max(this.BottomRight.X, area.BottomRight.X);
        var bottomY = Math.Max(this.BottomRight.Y, area.BottomRight.Y);

        return new Area(new Point(leftX, topY), new Point(rightX, bottomY));
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{TopLeft} ; {Size}";
}
