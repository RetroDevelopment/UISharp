namespace RetroDev.OpenUI.Core;

/// <summary>
/// Renders SVG images into a rgba image.
/// </summary>
public interface ISVGRenderingEngine
{
    /// <summary>
    /// Converts the given svg image into a rgba image, used by <see cref="IRenderingEngine"/> to render images.
    /// </summary>
    /// <param name="svgContent"></param>
    /// <returns></returns>
    RgbaImage ConvertSvgToRgba(string svgContent);
}
