using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Graphics.Shapes;

/// <summary>
/// Defines a generic shape to render.
/// </summary>
public interface IShape
{
    /// <summary>
    /// The shape background color.
    /// If <see langword="null"/>, the background will be transparent.
    /// </summary>
    public Color BackgroundColor { get; }

    /// <summary>
    /// The shape border color.
    /// If <see langword="null"/>, the border will be transparent.
    /// </summary>
    public Color BorderColor { get; }

    /// <summary>
    /// The border thickness in pixels.
    /// If <see langword="null" />, the border thickness will be 1 pixel.
    /// </summary>
    public PixelUnit? BorderThickness { get; }

    /// <summary>
    /// The image rotation in radians.
    /// </summary>
    public float Rotation { get; }

    /// <summary>
    /// The ID of the texture to display, or <see langword="null" /> if no texture is specified.
    /// </summary>
    public int? TextureID { get; }

    void Validate() { }
}
