using System;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Graphics.Imaging;
/// <summary>
/// Represents an RGBA image where each pixel has 4 color components (Red, Green, Blue, Alpha).
/// </summary>
public sealed class RgbaImage : Image
{
    /// <summary>
    /// The empty image.
    /// </summary>
    public static RgbaImage Empty { get; } = new(Size.Zero);

    /// <inheritdoc />
    public override uint BytesPerPixel => 4;

    /// <inheritdoc />
    internal override PixelInternalFormat InternalFormat => PixelInternalFormat.Rgba8;

    /// <inheritdoc />
    internal override PixelFormat PixelFormat => PixelFormat.Rgba;

    /// <inheritdoc />
    internal override PixelType PixelType => PixelType.UnsignedByte;


    /// <summary>
    /// Initializes a new instance of the <see cref="RgbaImage"/> class.
    /// </summary>
    /// <param name="size">The dimensions of the image.</param>
    public RgbaImage(Size size) : base(size) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RgbaImage"/> class.
    /// </summary>
    /// <param name="data">The image raw data.</param>
    /// <param name="size">The dimensions of the image.</param>
    public RgbaImage(byte[] data, Size size) : base(data, size) { }

    /// <summary>
    /// Sets an RGBA pixel at the specified location.
    /// </summary>
    /// <param name="point">The point (x, y) where the pixel should be set.</param>
    /// <param name="color">The color to set.</param>
    public void SetPixel(Point point, Color color)
    {
        if (point.X < 0 || point.X >= Size.Width || point.Y < 0 || point.Y >= Size.Height)
            throw new ArgumentOutOfRangeException(nameof(point), "Point is outside the image bounds.");

        int index = (int)(point.Y.Value * Size.Width.Value + point.X) * 4;
        Data[index] = color.RedComponent;
        Data[index + 1] = color.GreenComponent;
        Data[index + 2] = color.BlueComponent;
        Data[index + 3] = color.AlphaComponent;
    }
}
