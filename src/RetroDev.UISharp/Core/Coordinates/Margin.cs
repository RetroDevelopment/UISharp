namespace RetroDev.UISharp.Core.Coordinates;

/// <summary>
/// Defines margins within an area.
/// </summary>
/// <param name="Top">Margins from the top area.</param>
/// <param name="Right">Margins from the right area.</param>
/// <param name="Bottom">Margins from the bottom area.</param>
/// <param name="Left">Margins from the left area.</param>
public record struct Margin(PixelUnit? Top, PixelUnit? Right, PixelUnit? Bottom, PixelUnit? Left)
{
    /// <summary>
    /// Creates a new set of margins all set to <see cref="PixelUnit.Zero"/>.
    /// </summary>
    public Margin() : this(null, null, null, null) { }
}
