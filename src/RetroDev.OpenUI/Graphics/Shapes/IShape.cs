namespace RetroDev.OpenUI.Graphics.Shapes;

/// <summary>
/// Defines a generic shape to render.
/// </summary>
public interface IShape
{
    /// <summary>
    /// The shape background color.
    /// </summary>
    public Color BackgroundColor { get; }

    /// <summary>
    /// The ID of the texture to display, or <see langword="null" /> if no texture is specified.
    /// </summary>
    public int? TextureID { get; }
}
