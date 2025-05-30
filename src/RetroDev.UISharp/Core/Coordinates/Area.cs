﻿using System.Diagnostics;

namespace RetroDev.UISharp.Core.Coordinates;

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
        container is not null ? new(container.TopLeft + TopLeft, Size) : this;

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
    /// Creates a <see cref="Area"/> relative to <see langword="this" /> area that covers it completely.
    /// </summary>
    /// <remarks>
    /// This method is very useful when creating a drawing area relative to a container drawing area that fills it completely.
    /// For example, given a relative drawing area <c>a</c> with location (100, 100) with size 300 x 300, <c>a.Fill()</c> will return
    /// the area with location (0, 0) and size 300 x 300, which is the area relative to <c>a</c> that fully fills <c>a</c>.
    /// </remarks>
    /// <returns>The area with <see cref="Point.Zero"/> coordinate and the same <see cref="Size"/> as <see langword="this" /> area.</returns>
    public Area Fill() => Size.Fill();

    /// <summary>
    /// Merges <see langword="this" /> area with the given <paramref name="area"/>.
    /// </summary>
    /// <param name="area">The <see cref="Area"/> to merge.</param>
    /// <returns>The minimum area that contains both <see langword="this" /> area and the given <paramref name="area"/>.</returns>
    public Area Merge(Area area)
    {
        var leftX = Math.Min(TopLeft.X, area.TopLeft.X);
        var topY = Math.Min(TopLeft.Y, area.TopLeft.Y);
        var rightX = Math.Max(BottomRight.X, area.BottomRight.X);
        var bottomY = Math.Max(BottomRight.Y, area.BottomRight.Y);

        return new Area(new Point(leftX, topY), new Point(rightX, bottomY));
    }

    /// <summary>
    /// Clamps an area within a container, ensuring it respects the given margins.
    /// The top-left corner is adjusted if it's within the margin boundaries, 
    /// and the size is clamped to ensure it does not exceed the available space.
    /// </summary>
    /// <param name="containerSize">The total size of the container.</param>
    /// <param name="margin">The margins to apply around the area.</param>
    /// <returns>A new clamped area respecting the margins.</returns>
    public Area Clamp(Size containerSize, Margin margin)
    {
        var minX = margin.Left.IfAuto(PixelUnit.Min);
        var maxX = Math.Clamp((containerSize.Width - margin.Right).IfAuto(PixelUnit.Max), minX, PixelUnit.Max);
        var minY = margin.Top.IfAuto(PixelUnit.Min);
        var maxY = Math.Clamp((containerSize.Height - margin.Bottom).IfAuto(PixelUnit.Max), minY, PixelUnit.Max);

        var left = Math.Clamp(TopLeft.X, minX, maxX);
        var right = Math.Clamp(left + Size.Width, minX, maxX);
        var top = Math.Clamp(TopLeft.Y, minY, maxY);
        var bottom = Math.Clamp(top + Size.Height, minY, maxY);

        return new Area(topLeft: new Point(left, top), bottomRight: new Point(right, bottom));
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{TopLeft} ; {Size}";
}
