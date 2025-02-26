using OpenTK.Graphics.OpenGL;
using RetroDev.UISharp.Core.Graphics.Coordinates;

namespace RetroDev.UISharp.Core.Graphics.Imaging;

/// <summary>
/// Represents a grayscale image where each pixel has a single intensity value.
/// </summary>
public sealed class GrayscaleImage : Image
{
    /// <summary>
    /// The empty image.
    /// </summary>
    public static GrayscaleImage Empty { get; } = new(Size.Zero);

    /// <inheritdoc />
    public override uint BytesPerPixel => 1;

    /// <inheritdoc />
    internal override PixelInternalFormat InternalFormat => PixelInternalFormat.R8;

    /// <inheritdoc />
    internal override PixelFormat PixelFormat => PixelFormat.Red;

    /// <inheritdoc />
    internal override PixelType PixelType => PixelType.UnsignedByte;


    /// <summary>
    /// Initializes a new instance of the <see cref="GrayscaleImage"/> class.
    /// </summary>
    /// <param name="size">The dimensions of the image.</param>
    public GrayscaleImage(Size size) : base(size) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="GrayscaleImage"/> class.
    /// </summary>
    /// <param name="data">The image raw data.</param>
    /// <param name="size">The dimensions of the image.</param>
    public GrayscaleImage(byte[] data, Size size) : base(data, size) { }

    /// <summary>
    /// Sets a grayscale pixel at the specified location.
    /// </summary>
    /// <param name="point">The point (x, y) where the pixel should be set.</param>
    /// <param name="color">The grayscale color intensity to set.</param>
    public void SetPixel(Point point, byte color)
    {
        if (point.X < 0 || point.X >= Size.Width || point.Y < 0 || point.Y >= Size.Height)
            throw new ArgumentOutOfRangeException(nameof(point), "Point is outside the image bounds.");

        Data[(int)(point.Y * Size.Width + point.X)] = color;
    }

    /// <summary>
    /// Copies the given <paramref name="sourceImage"/> into <see langword="this" /> <see cref="GrayscaleImage">
    /// at the specified <paramref name="offset"/>.
    /// Uses <see cref="Buffer.MemoryCopy"/> for optimal performance.
    /// </summary>
    /// <paramref name="sourceImage"/></summary>
    /// 

    /// <summary>
    /// Copies the given <paramref name="sourceImage"/> into <see langword="this" /> <see cref="GrayscaleImage">
    /// at the specified <paramref name="offset"/>.
    /// Uses <see cref="Buffer.MemoryCopy"/> for optimal performance.
    /// </summary>
    /// <param name="sourceImage">The image to copy.</param>
    /// <param name="offset">
    /// The offset where to copy the image. For example (10, 10) means that the top-left of the
    /// image to copy will be located at (10, 10) pixels from <see langword="this" /> image top-left.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the <paramref name="sourceImage"/> to copy does not fit <see langword="this" /> image size because it is
    /// too big or the offset would cause the <paramref name="sourceImage"/> to go outside of <see cref="this"/> image boundaries.
    /// </exception>
    public unsafe void CopyFrom(GrayscaleImage sourceImage, Point offset)
    {
        // Ensure the glyph fits within the image
        if (offset.X < 0 || offset.Y < 0 ||
            offset.X + sourceImage.Size.Width > Size.Width ||
            offset.Y + sourceImage.Size.Height > Size.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Data to copy exceeds image boundaries.");
        }

        var sourceDataWidth = (int)sourceImage.Size.Width;
        var sourceDataHeight = (int)sourceImage.Size.Height;
        var destinationDataWidth = (int)Size.Width;
        var offsetX = (int)offset.X;
        var offsetY = (int)offset.Y;

        fixed (byte* srcPtr = sourceImage.Data)
        fixed (byte* dstPtr = Data)
        {
            for (var y = 0; y < sourceDataHeight; y++)
            {
                var srcRow = srcPtr + (y * sourceDataWidth);
                var dstRow = dstPtr + ((offsetY + y) * destinationDataWidth + offsetX);

                System.Buffer.MemoryCopy(srcRow, dstRow, sourceDataWidth, sourceDataWidth);
            }
        }
    }
}
