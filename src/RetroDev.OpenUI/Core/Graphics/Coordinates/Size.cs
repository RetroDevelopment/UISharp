using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Graphics.Coordinates;

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
    /// <returns>An <see cref="Area"/> that fills the entire container.</returns>
    public Area Fill() =>
        new(Point.Zero, this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the top-left corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> positioned at (0,0) with the same size as this instance.</returns>
    public Area FillTopLeftOf(Size containerSize) =>
        new(Point.Zero, this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center of the top edge of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> centered horizontally at the top of the container.</returns>
    public Area FillTopCenterOf(Size containerSize) =>
        new(new Point((containerSize.Width - Width) / 2.0f, PixelUnit.Zero), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the top-right corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> aligned to the top-right of the container.</returns>
    public Area FillTopRightOf(Size containerSize) =>
        new(new Point(containerSize.Width - Width, PixelUnit.Zero), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center-left of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> aligned to the center-left of the container.</returns>
    public Area FillCenterLeftOf(Size containerSize) =>
        new(new Point(PixelUnit.Zero, (containerSize.Height - Height) / 2.0f), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the exact center of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> centered within the container.</returns>
    public Area FillCenterOf(Size containerSize) =>
        new(new Point((containerSize.Width - Width) / 2.0f, (containerSize.Height - Height) / 2.0f), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center-right of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> aligned to the center-right of the container.</returns>
    public Area FillCenterRightOf(Size containerSize) =>
        new(new Point(containerSize.Width - Width, (containerSize.Height - Height) / 2.0f), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the bottom-left corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> aligned to the bottom-left of the container.</returns>
    public Area FillBottomLeftOf(Size containerSize) =>
        new(new Point(PixelUnit.Zero, containerSize.Height - Height), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the center of the bottom edge of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> centered horizontally at the bottom of the container.</returns>
    public Area FillBottomCenterOf(Size containerSize) =>
        new(new Point((containerSize.Width - Width) / 2.0f, containerSize.Height - Height), this);

    /// <summary>
    /// Creates an <see cref="Area"/> positioned at the bottom-right corner of a given container.
    /// </summary>
    /// <param name="containerSize">The size of the container.</param>
    /// <returns>An <see cref="Area"/> aligned to the bottom-right of the container.</returns>
    public Area FillBottomRightOf(Size containerSize) =>
        new(new Point(containerSize.Width - Width, containerSize.Height - Height), this);

    public static Size operator +(Size s1, Size s2) => new(s1.Width + s2.Width, s1.Height + s2.Height);
    public static Size operator -(Size s1, Size s2) => new(s1.Width - s2.Width, s1.Height - s2.Height);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Width.Value} x {Height.Value}";
}
