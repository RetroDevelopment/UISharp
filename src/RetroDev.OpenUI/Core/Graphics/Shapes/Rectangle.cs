using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Core.Graphics.Shapes;

/// <summary>
/// A rectangle.
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
/// <param name="CornerRadiusX">
/// The rectangle corner x-radius in pixels.
/// If omitted or zero, no rounded corners will be displyed.
/// </param>
/// <param name="CornerRadiusY">
/// The rectangle corner y-radius in pixels.
/// If omitted or zero, no rounded corners will be displyed.
/// </param>
/// <param name="Rotation">
/// The rectangle rotation in radians.
/// </param>
/// <param name="TextureID">
/// The ID of the texture to display, or <see langword="null" /> if no texture is specified.
/// </param>
public record Rectangle(Color BackgroundColor = default,
                        Color BorderColor = default,
                        PixelUnit? BorderThickness = null,
                        PixelUnit? CornerRadiusX = null,
                        PixelUnit? CornerRadiusY = null,
                        float Rotation = 0.0f,
                        int? TextureID = null) : IShape;
