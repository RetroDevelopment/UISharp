using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;

namespace RetroDev.OpenUI.Core;

/// <summary>
/// The rendering engine used to render on a given window.
/// </summary>
public interface IRenderingEngine
{
    /// <summary>
    /// Creates a texture with the given <paramref name="image"/> and stores it in memory.
    /// </summary>
    /// <param name="image">An RGBA image.</param>
    /// <returns>The store texture unique identifier used when referencing this texture.</returns>
    int CreateTexture(RgbaImage image);

    /// <summary>
    /// Renders a rectangle.
    /// </summary>
    /// <param name="rectangle">The triangle shape attributes.</param>
    /// <param name="area">The rectangle rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    void Render(Rectangle rectangle, Area area, Area? clippingArea);

    /// <summary>
    /// Renders a circle.
    /// </summary>
    /// <param name="circle">The circle shape attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    void Render(Circle circle, Area area, Area? clippingArea);

    /// <summary>
    /// Renders text.
    /// </summary>
    /// <param name="text">The text attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    void Render(Text text, Area area, Area? clippingArea);

    /// <summary>
    /// This method is invoked when starting the rendering of a frame.
    /// </summary>
    /// <param name="backgroundColor">
    /// The frame background color.
    /// </param>
    void InitializeFrame(Color backgroundColor);

    /// <summary>
    /// This method is called when the frame rendering is complete.
    /// </summary>
    void FinalizeFrame();

    /// <summary>
    /// Deallocates all the rendering resources.
    /// </summary>
    void Shutdown();
}
