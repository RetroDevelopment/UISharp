using System.Runtime.CompilerServices;

namespace RetroDev.UISharp.Core.Coordinates;

/// <summary>
/// Defines margins within an area.
/// </summary>
/// <param name="Top">Margins from the top area.</param>
/// <param name="Right">Margins from the right area.</param>
/// <param name="Bottom">Margins from the bottom area.</param>
/// <param name="Left">Margins from the left area.</param>
public record struct Margin(PixelUnit Top, PixelUnit Right, PixelUnit Bottom, PixelUnit Left)
{
    /// <summary>
    /// Represents a Margin with all sides set to <see cref="PixelUnit.Auto"/>.
    /// </summary>
    public static readonly Margin Auto = new Margin(PixelUnit.Auto, PixelUnit.Auto, PixelUnit.Auto, PixelUnit.Auto);

    /// <summary>
    /// Whether <see langword="this" /> margin has all values set to <see cref="PixelUnit.Auto"/>.
    /// </summary>
    public bool IsAuto => this == Auto;
}
