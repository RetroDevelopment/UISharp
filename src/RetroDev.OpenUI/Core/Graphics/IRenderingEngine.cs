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
    /// Adds the given <paramref name="rectangle"/> to the rendering canvas.
    /// </summary>
    /// <param name="rectangle">The rectangle to add.</param>
    void Add(Rectangle rectangle);

    /// <summary>
    /// Adds the given <paramref name="circle"/> to the rendering canvas.
    /// </summary>
    /// <param name="circle">The circle to add.</param>
    void Add(Circle circle);

    /// <summary>
    /// Adds the given <paramref name="text"/> to the rendering canvas.
    /// </summary>
    /// <param name="text">The text to add.</param>
    void Add(Text text);

    /// <summary>
    /// Removes the given <paramref name="rectangle"/> from the rendering canvas.
    /// </summary>
    /// <param name="rectangle">The rectangle to add.</param>
    void Remove(Rectangle rectangle);

    /// <summary>
    /// Removes the given <paramref name="circle"/> from the rendering canvas.
    /// </summary>
    /// <param name="circle">The circle to add.</param>
    void Remove(Circle circle);

    /// <summary>
    /// Removes the given <paramref name="text"/> from the rendering canvas.
    /// </summary>
    /// <param name="text">The text to add.</param>
    void Remove(Text text);

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
    /// <remarks>
    /// This is where all the <see cref="RenderingElement"/> added are actually rendered.
    /// For performance reason the modified elements need to be re-processed for redrwaing.
    /// This is what enables retained mode in the UI.
    /// </remarks>
    void FinalizeFrame();

    /// <summary>
    /// Deallocates all the rendering resources.
    /// </summary>
    void Shutdown();
}
