using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Core.Graphics.Shapes;

/// <summary>
/// A circle.
/// </summary>
/// <param name="BackgroundColor">
/// The shape background color.
/// If omitted, the background will be transparent.</param>
/// <param name="BorderColor">
/// The shape border color.
/// If omitted, the border will be transparent.
/// </param>
/// <param name="BorderThickness">
/// The border thickness in pixels.
/// If omitted, the border thickness will be 1 pixel.
/// </param>
/// <param name="Rotation">
/// The circle rotation in radians.
/// </param>
/// <param name="TextureID">
/// The ID of the texture to display, or <see langword="null" /> if no texture is specified.
/// </param>
public record Circle(Color BackgroundColor = default,
                     Color BorderColor = default,
                     PixelUnit? BorderThickness = default,
                     float Rotation = 0.0f,
                     int? TextureID = default) : IShape;
