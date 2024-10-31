using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;

namespace RetroDev.OpenUI.Core;

/// <summary>
/// Allows to draw on a UI component surface.
/// </summary>
public class Canvas
{
    private readonly IRenderingEngine _renderingEngine;
    private readonly ISVGRenderingEngine _svgEngine;
    private readonly IFontRenderingEngine _fontEngine;
    private readonly LifeCycle _lifeCycle;

    internal Area AbsoluteDrawingArea { get; set; }

    /// <summary>
    /// Creates a new canvas.
    /// </summary>
    /// <param name="renderingEngine">The rendering engine used to draw.</param>
    /// <param name="svgEngine">The object converting svg images to rgba byte arrays.</param>
    /// <param name="fontEngine">The object converting text to rgba byte arrays.</param>
    /// <param name="lifeCycle">The life cycle management object ensuring that rendering operations are perfomed when it is allowed.</param>
    internal Canvas(IRenderingEngine renderingEngine, ISVGRenderingEngine svgEngine, IFontRenderingEngine fontEngine, LifeCycle lifeCycle)
    {
        _renderingEngine = renderingEngine;
        _svgEngine = svgEngine;
        _fontEngine = fontEngine;
        _lifeCycle = lifeCycle;
        AbsoluteDrawingArea = new(Point.ZERO, Size.ZERO);
    }

    /// <summary>
    /// Creates and caches a texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="image">The rgba image.</param>
    /// <exception cref="ArgumentException">If a texture with the same <paramref name="textureIdentifier"/> exists.</exception>
    /// <exception cref="RenderingException">If the texture cannot be created because of a system error.</exception>
    public void CreateTexture(string textureIdentifier, RgbaImage image) =>
        _renderingEngine.CreateTexture(textureIdentifier, image);

    /// <summary>
    /// Updates an existing texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="image">The rgba image.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist or the rgba array doesn't have the same size as the originally created texture.</exception>
    /// <exception cref="RenderingException">If the texture cannot be updated because of a system error.</exception>
    public void UpdateTexture(string textureIdentifier, RgbaImage image) =>
        _renderingEngine.UpdateTexture(textureIdentifier, image);

    /// <summary>
    /// Deletes an existing texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    public void DeleteTexture(string textureIdentifier) =>
        _renderingEngine.DeleteTexture(textureIdentifier);

    /// <summary>
    /// Checks whether the given texture has been created with <see cref="CreateTexture(string, byte[], CoordinateType, CoordinateType)"/>.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <returns><see langword="true"/> if the texture exists, otherwise <see langword="false"/>.</returns>
    public bool TextureExists(string textureIdentifier) =>
        _renderingEngine.TextureExists(textureIdentifier);

    /// <summary>
    /// Renders a texture in the viewport.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="renderingArea">The area where to render the texture. If the area size is differnet from the texture size the image will be streteched.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    public void RenderTexture(string textureIdentifier, Area renderingArea)
    {
        var clippingArea = AbsoluteDrawingArea;
        var absoluteRenderingArea = new Area(clippingArea.TopLeft + renderingArea.TopLeft, renderingArea.Size);
        _renderingEngine.RenderTexture(textureIdentifier, absoluteRenderingArea, clippingArea);
    }

    /// <summary>
    /// Creates a texture from the given svg <paramref name="image"/>.
    /// </summary>
    /// <param name="textureIdentifier">THe unique texture identifier.</param>
    /// <param name="image">The svg content of the image to render.</param>
    /// <param name="renderingArea">The area where to render the texture. If the area size is differnet from the texture size the image will be streteched.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    public void CreateSvgTexture(string textureIdentifier, string image)
    {
        var rgbaImage = _svgEngine.ConvertSvgToRgba(image);
        _renderingEngine.CreateTexture(textureIdentifier, rgbaImage);
    }

    /// <summary>
    /// Creates a texture from the given svg <paramref name="text"/>.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="text">The text content of the image to render.</param>
    /// <param name="fontSize">The font size of the text to draw.</param>
    /// <returns>The (width, height) pair, representing the original width and height text texture.</returns>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    public (int width, int height) CreateTextTexture(string textureIdentifier, string text, int fontSize)
    {
        var rgbaImage = _fontEngine.ConvertTextToRgbaImage(text, fontSize);
        _renderingEngine.CreateTexture(textureIdentifier, rgbaImage);
        return (width: rgbaImage.Width, height: rgbaImage.Height);
    }
}
