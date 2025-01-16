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
    /// <summary>
    /// The empty area which requires 0 pizels.
    /// </summary>
    public static Area Empty => new(Point.Zero, Point.Zero);

    /// <summary>
    /// The area central point coordinate in pixels.
    /// </summary>
    public Point Center => new(TopLeft.X + Size.Width / 2, TopLeft.Y + Size.Height / 2);


    /// <summary>
    /// The area bottom-right point coordinate in pixels.
    /// </summary>
    public Point BottomRight => new(TopLeft.X + Size.Width, TopLeft.Y + Size.Height);

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

        var oneOffset = new Point(1); // Due to some approximation errors (especially in GridLayout) increase the clip area by 1. The has shown to improve the UI availding some hard cuts of images without introducing visible unclipped areas.
        var clippedTopLeft = TopLeft.Clamp(container.TopLeft, container.BottomRight);
        var clippedBottomRight = BottomRight.Clamp(container.TopLeft + oneOffset, container.BottomRight + oneOffset);

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
}
