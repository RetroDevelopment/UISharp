using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

/// <summary>
/// Represents a 2D-Area.
/// </summary>
[DebuggerDisplay("{TopLeft} - {Size}")]
public record Area(Point TopLeft, Size Size)
{
    /// <summary>
    /// Clips this area so that it fits in the given <paramref name="container"/> area.
    /// </summary>
    /// <param name="container">The area containing <see langword="this"/> area.</param>
    /// <returns>The clipped area.</returns>
    public Area Clip(Area? container)
    {
        if (container == null) return this;

        var bottomRightX = TopLeft.X + Size.Width;
        var bottomRightY = TopLeft.Y + Size.Height;
        var containerBottomRightX = container.TopLeft.X + container.Size.Width;
        var containerBottomRightY = container.TopLeft.Y + container.Size.Height;

        var width = Size.Width;
        var height = Size.Height;

        if (bottomRightX > containerBottomRightX)
        {
            width -= bottomRightX - containerBottomRightX;
        }

        if (bottomRightY > containerBottomRightY)
        {
            height -= bottomRightY - containerBottomRightY;
        }

        return new(TopLeft, new(width, height));
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
