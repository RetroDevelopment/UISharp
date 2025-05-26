using OpenTK.Graphics.OpenGL;
using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Core.Graphics.Imaging;

/// <summary>
/// An abstract class representing an image. Derived classes will implement a specific encoding.
/// </summary>
public abstract class Image
{
    /// <summary>
    /// Gets the size of the image.
    /// </summary>
    public Size Size { get; }

    /// <summary>
    /// Gets the image raw data.
    /// </summary>
    public byte[] Data { get; }

    /// <summary>
    /// The number of bytes per pixel.
    /// </summary>
    public abstract uint BytesPerPixel { get; }

    /// <summary>
    /// Gets the OpenGL internal format (e.g., Rgba8, R8, etc.).
    /// </summary>
    internal abstract PixelInternalFormat InternalFormat { get; }

    /// <summary>
    /// Gets the OpenGL pixel format (e.g., Rgba, Red, etc.).
    /// </summary>
    internal abstract PixelFormat PixelFormat { get; }

    /// <summary>
    /// Gets the OpenGL pixel type (e.g., UnsignedByte).
    /// </summary>
    internal abstract PixelType PixelType { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="size">The dimensions of the image.</param>
    /// <remarks>
    /// It is not allowed to derive from this class outside of this library, because <see cref="Data"/> will be
    /// sent to opengl so control over what data flows into fragment shader is needed.
    /// For this reason, this constructor is private protected.
    /// </remarks>
    private protected Image(Size size)
    {
        Size = size;
        Data = new byte[(int)size.Width.Value * (int)size.Height.Value * BytesPerPixel];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Image"/> class.
    /// </summary>
    /// <param name="data">The image raw data.</param>
    /// <param name="size">The dimensions of the image.</param>
    /// <remarks>
    /// It is not allowed to derive from this class outside of this library, because <see cref="Data"/> will be
    /// sent to opengl so control over what data flows into fragment shader is needed.
    /// For this reason, this constructor is private protected.
    /// </remarks>
    private protected Image(byte[] data, Size size)
    {
        var expectedRawDataSize = size.Width * size.Height * BytesPerPixel;
        if (data.Length != expectedRawDataSize)
            throw new ArgumentException($"Invalid raw data size {data.Length} for a {size.Width} x {size.Height} pixels image: expected {expectedRawDataSize} bytes.", nameof(data));

        Data = data;
        Size = size;
    }
}
