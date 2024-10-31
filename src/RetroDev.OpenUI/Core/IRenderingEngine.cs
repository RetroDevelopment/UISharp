using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.Core;

/// <summary>
/// The rendering engine used to render on a given window.
/// </summary>
public interface IRenderingEngine
{
    /// <summary>
    /// Creates and caches a texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="image">The rgba image.</param>
    /// <exception cref="ArgumentException">If a texture with the same <paramref name="textureIdentifier"/> exists.</exception>
    /// <exception cref="RenderingException">If the texture cannot be created because of a system error.</exception>
    void CreateTexture(string textureIdentifier, RgbaImage image);

    /// <summary>
    /// Updates an existing texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="image">The rgba image.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist or the rgba array doesn't have the same size as the originally created texture.</exception>
    /// <exception cref="RenderingException">If the texture cannot be updated because of a system error.</exception>
    void UpdateTexture(string textureIdentifier, RgbaImage image);

    /// <summary>
    /// Deletes an existing texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    void DeleteTexture(string textureIdentifier);

    /// <summary>
    /// Checks whether the given texture has been created with <see cref="CreateTexture(string, byte[], CoordinateType, CoordinateType)"/>.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <returns><see langword="true"/> if the texture exists, otherwise <see langword="false"/>.</returns>
    bool TextureExists(string textureIdentifier);

    /// <summary>
    /// Renders a texture in the viewport.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="renderingArea">The area where to render the texture. If the area size is differnet from the texture size the image will be streteched.</param>
    /// <param name="clippingArea">The area where the texture can be rendered. All the pixels outside of that area will be clipped.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    void RenderTexture(string textureIdentifier, Area renderingArea, Area clippingArea);

    /// <summary>
    /// This method is invoked when starting the rendering of a frame.
    /// </summary>
    void InitializeFrame();

    /// <summary>
    /// This method is called when the frame rendering is complete.
    /// </summary>
    void FinalizeFrame();

    /// <summary>
    /// Deallocates all the rendering resources.
    /// </summary>
    void Shutdown();
}
