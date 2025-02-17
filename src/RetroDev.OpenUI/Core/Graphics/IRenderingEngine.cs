using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Contexts;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Graphics.Fonts;
using RetroDev.OpenUI.Core.Graphics.Imaging;
using RetroDev.OpenUI.Core.Graphics.Shapes;

namespace RetroDev.OpenUI.Core.Graphics;

/// <summary>
/// The rendering engine used to render on a given window.
/// </summary>
public interface IRenderingEngine
{
    /// <summary>
    /// The víewport size in pixels.
    /// </summary>
    Size ViewportSize { get; set; }

    /// <summary>
    /// The rendering context used by this engine to abstract away from window.
    /// </summary>
    IRenderingContext RenderingContext { get; }

    /// <summary>
    /// Creates a texture with the given <paramref name="image"/> and stores it in memory.
    /// </summary>
    /// <param name="image">An RGBA image.</param>
    /// <param name="interpolate">Whether to interpolate the image or render it as is.</param>
    /// <returns>The store texture unique identifier used when referencing this texture.</returns>
    int CreateTexture(Image image, bool interpolate);

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
    /// Calculates the size to display the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="font">The text font.</param>
    /// <returns>The size to correctly and fully display the given <paramref name="text"/>.</returns>
    Size ComputeTextSize(string text, Font font);

    /// <summary>
    /// Gets the maximum height occupied by a line of text using the given <paramref name="font"/>.
    /// </summary>
    /// <param name="font">The font for which to compute the height.</param>
    /// <returns>The minimum height necessary to render any character using the given <paramref name="font"/>.</returns>
    PixelUnit ComputeTextMaximumHeight(Font font);

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
