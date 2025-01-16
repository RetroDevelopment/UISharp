using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events.Internal;
using RetroDev.OpenUI.Graphics.Shapes;

namespace RetroDev.OpenUI.Graphics;

/// <summary>
/// Allows to draw on a UI component surface.
/// </summary>
public class Canvas
{
    private readonly IRenderingEngine _renderingEngine;
    private readonly LifeCycle _lifeCycle;

    internal Area? ClippingArea { get; set; }
    internal Area ContainerAbsoluteDrawingArea { get; set; } = Area.Empty;

    /// <summary>
    /// Creates a new canvas.
    /// </summary>
    /// <param name="renderingEngine">The rendering engine used to draw.</param>
    /// <param name="lifeCycle">The life cycle management object ensuring that rendering operations are perfomed when it is allowed.</param>
    internal Canvas(IRenderingEngine renderingEngine, LifeCycle lifeCycle)
    {
        _renderingEngine = renderingEngine;
        _lifeCycle = lifeCycle;
    }

    /// <summary>
    /// Creates a texture with the given <paramref name="image"/> and stores it in memory.
    /// </summary>
    /// <param name="image">An RGBA image.</param>
    /// <returns>The store texture unique identifier used when referencing this texture.</returns>
    public int CreateTexture(RgbaImage image)
    {
        _lifeCycle.ThrowIfNotOnRenderingPhase();
        return _renderingEngine.CreateTexture(image);
    }

    /// <summary>
    /// Renders a rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle shape attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    public void Render(Rectangle rectangle, Area area)
    {
        _lifeCycle.ThrowIfNotOnRenderingPhase();
        _renderingEngine.Render(rectangle, area.ToAbsolute(ContainerAbsoluteDrawingArea), ClippingArea);
    }

    /// <summary>
    /// Renders a circle.
    /// </summary>
    /// <param name="circle">The circle shape attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    public void Render(Circle circle, Area area)
    {
        _lifeCycle.ThrowIfNotOnRenderingPhase();
        _renderingEngine.Render(circle, area.ToAbsolute(ContainerAbsoluteDrawingArea), ClippingArea);
    }

    /// <summary>
    /// Renders text.
    /// </summary>
    /// <param name="text">The text attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    public void Render(Text text, Area area)
    {
        _lifeCycle.ThrowIfNotOnRenderingPhase();
        _renderingEngine.Render(text, area.ToAbsolute(ContainerAbsoluteDrawingArea), ClippingArea);
    }
}
