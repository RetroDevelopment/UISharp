﻿using System.Diagnostics;

namespace RetroDev.UISharp.Core.Coordinates;

/// <summary>
/// Represents a 2D area size.
/// </summary>
[DebuggerDisplay("{Width} x {Height}")]
public record Size
{
    /// <summary>
    /// Size that requires zero pixels.
    /// </summary>
    public static readonly Size Zero = new(0, 0);

    /// <summary>
    /// The maximum possible size.
    /// </summary>
    public static readonly Size Max = new(PixelUnit.Max, PixelUnit.Max);

    /// <summary>
    /// The width in pixels.
    /// </summary>
    public PixelUnit Width { get; }

    /// <summary>
    /// The height in pixels.
    /// </summary>
    public PixelUnit Height { get; }

    /// <summary>
    /// Creates a new size.
    /// </summary>
    /// <param name="width">The width in pixels (it must be greater or equal to zero).</param>
    /// <param name="height">The height in pixels (it must be greater or equal to zero).></param>
    public Size(PixelUnit width, PixelUnit height)
    {
        if (width.Value < 0.0f) throw new ArgumentException($"With must be greater or equal to zero, {width} found.");
        if (height.Value < 0.0f) throw new ArgumentException($"Height must be greater or equal to zero, {height} found.");

        Width = width;
        Height = height;
    }

    /// <summary>
    /// Creates an <see cref="Area"/> from (0,0) with the same size as this instance.
    /// </summary>
    /// <param name="margin">The margin to leave when filling this area.</param>
    /// <returns>An <see cref="Area"/> that fills the entire container.</returns>
    public Area Fill(Margin? margin = null) =>
        new Area(Point.Zero, this).Clamp(this, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the top-left corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> positioned at (0,0) with the same size as this instance.</returns>
    public Area PositionTopLeftOf(Size containerSize, Margin? margin = null) =>
        new Area(Point.Zero, this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center of the top edge of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> centered horizontally at the top of the container.</returns>
    public Area PositionTopCenterOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point((containerSize.Width - Width) / 2.0f, PixelUnit.Zero), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the top-right corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> aligned to the top-right of the container.</returns>
    public Area PositionTopRightOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point(containerSize.Width - Width, PixelUnit.Zero), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center-left of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> aligned to the center-left of the container.</returns>
    public Area PositionCenterLeftOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point(PixelUnit.Zero, (containerSize.Height - Height) / 2.0f), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the exact center of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> centered within the container.</returns>
    public Area PositionCenterOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point((containerSize.Width - Width) / 2.0f, (containerSize.Height - Height) / 2.0f), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center-right of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> aligned to the center-right of the container.</returns>
    public Area PositionCenterRightOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point(containerSize.Width - Width, (containerSize.Height - Height) / 2.0f), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the bottom-left corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> aligned to the bottom-left of the container.</returns>
    public Area PositionBottomLeftOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point(PixelUnit.Zero, containerSize.Height - Height), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center of the bottom edge of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> centered horizontally at the bottom of the container.</returns>
    public Area PositionBottomCenterOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point((containerSize.Width - Width) / 2.0f, containerSize.Height - Height), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the bottom-right corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <param name="margin">The margin to leave when positioning the area with <see langword="this" /> <see cref="Size"/>.</param>
    /// <returns>An <see cref="Area"/> aligned to the bottom-right of the container.</returns>
    public Area PositionBottomRightOf(Size containerSize, Margin? margin = null) =>
        new Area(new Point(containerSize.Width - Width, containerSize.Height - Height), this).Clamp(containerSize, margin ?? Margin.Auto);

    /// <summary>
    /// Inflates <see langword="this" /> <see cref="Size"/> taking margins into account.
    /// For example a 100 x 100 size with left margin of 10 and right margins of 20 will be inflated into an area 130 x 100.
    /// </summary>
    /// <param name="margin">The margin for inflation.</param>
    /// <returns>A <see cref="Size"/> which is bigger than <see langword="this" /> to accomodate the specified <paramref name="margin"/>.</returns>
    public Size Inflate(Margin margin) =>
        new Size(Width + (margin.Left.IfAuto(PixelUnit.Zero)) + (margin.Right.IfAuto(PixelUnit.Zero)),
                 Height + (margin.Top.IfAuto(PixelUnit.Zero)) + (margin.Bottom.IfAuto(PixelUnit.Zero)))
           .Clamp(Zero, Max);

    /// <summary>
    /// Deflates <see langword="this" /> <see cref="Size"/> taking padding into account.
    /// For example a 100 x 100 size with left padding of 10 and right padding of 20 will be deflated into an area 60 x 100.
    /// </summary>
    /// <param name="paddding">The padding for deflation.</param>
    /// <returns>A <see cref="Size"/> which is smaller than <see langword="this" /> to accomodate the specified <paramref name="padding"/>.</returns>
    public Size Deflate(Margin padding) =>
        new Size(Width - (padding.Left.IfAuto(PixelUnit.Zero)) - (padding.Right.IfAuto(PixelUnit.Zero)),
                 Height - (padding.Top.IfAuto(PixelUnit.Zero)) - (padding.Bottom.IfAuto(PixelUnit.Zero)))
           .Clamp(Zero, Max);

    /// <summary>
    /// Clamps <see langword="this" /> <see cref="Size"/> to the given <paramref name="minimumSize"/> and <paramref name="maximumSize"/>.
    /// </summary>
    /// <param name="minimumSize">The minimum allowed size.</param>
    /// <param name="maximumSize">The maximum allowed size.</param>
    /// <returns></returns>
    public Size Clamp(Size minimumSize, Size maximumSize) =>
        new(Math.Clamp(Width, minimumSize.Width.IsAuto ? PixelUnit.Min : minimumSize.Width, maximumSize.Width.IsAuto ? PixelUnit.Max : maximumSize.Width),
            Math.Clamp(Height, minimumSize.Height.IsAuto ? PixelUnit.Min : minimumSize.Height, maximumSize.Height.IsAuto ? PixelUnit.Auto : maximumSize.Height));

    public static Size operator +(Size s1, Size s2) => new(s1.Width + s2.Width, s1.Height + s2.Height);
    public static Size operator -(Size s1, Size s2) => new(s1.Width - s2.Width, s1.Height - s2.Height);
    public static Size operator +(Size s, PixelUnit u) => new(s.Width + u, s.Height + u);
    public static Size operator -(Size s, PixelUnit u) => new(s.Width - u, s.Height - u);
    public static Size operator *(Size s, PixelUnit u) => new(s.Width * u, s.Height * u);
    public static Size operator /(Size s, PixelUnit u) => new(s.Width / u, s.Height / u);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Width.Value} x {Height.Value}";
}
