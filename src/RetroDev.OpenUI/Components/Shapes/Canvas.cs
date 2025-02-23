using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Logging;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// Allows to draw on a UI component surface.
/// </summary>
public class Canvas
{
    private readonly ILogger _logger;
    public readonly List<UIShape> _shapes = [];
    public readonly HashSet<UIShape> _invalidatedShapes = [];

    /// <summary>
    /// The <see cref="UIComponent"/> owining <see langword="this" /> <see cref="Canvas"/>.
    /// </summary>
    public UIComponent Component { get; set; }

    /// <summary>
    /// The size of the area where <see cref="Component"/> is rendered.
    /// </summary>
    public Size ComponentSize { get; set; } = Size.Zero;

    internal Area ContainerAbsoluteDrawingArea { get; set; } = Area.Empty;

    internal Area ClippingArea { get; set; } = Area.Empty;

    internal IRenderingEngine? RenderingEngine { get; private set; }

    /// <summary>
    /// Creates a new canvas.
    /// </summary>
    /// <param name="component">The <see cref="UIComponent"/> where <see langword="this" /> <see cref="Canvas"/> is rendering.</param>
    internal Canvas(UIComponent component)
    {
        Component = component;
        _logger = component.Application.Logger;
    }

    /// <summary>
    /// Adds thie given <paramref name="shape"/> to <see langword="this" /> <see cref="Canvas"/>.
    /// </summary>
    /// <param name="shape">The shape to add.</param>
    public void Add(UIShape shape)
    {
        _logger.LogVerbose($"Adding shape {shape}");
        _shapes.Add(shape);
        shape.Canvas = this;
        shape.AddShapeToRenderingEngine();
        shape.Visible.Value = Component.Visibility.Value == UIComponent.ComponentVisibility.Visible;
        shape.Invalidate();
    }

    /// <summary>
    /// Removes the given <paramref name="shape"/> to from <see langword="this" /> <see cref="Canvas"/>.
    /// </summary>
    /// <param name="shape">The shape to add.</param>
    public void Remove(UIShape shape)
    {
        _logger.LogVerbose($"Removing shape {shape}");
        _shapes.Remove(shape);
        shape.Canvas = null;
        _invalidatedShapes.Remove(shape);
        shape.Canvas = null;
        shape.RemoveShapeFromRenderingEngine();
    }

    internal void Attach(IRenderingEngine renderingEngine)
    {
        Detach();
        RenderingEngine = renderingEngine;
        foreach (var shape in _shapes)
        {
            shape.AddShapeToRenderingEngine();
        }
    }

    internal void Detach()
    {
        if (RenderingEngine == null) return;

        foreach (var shape in _invalidatedShapes)
        {
            shape.RemoveShapeFromRenderingEngine();
        }

        RenderingEngine = null;
    }

    internal void Render(Area clipArea)
    {
        foreach (var shape in _invalidatedShapes)
        {
            shape.Render(clipArea);
        }
    }

    internal void AddInvalidatedShape(UIShape shape)
    {
        Component.Invalidate();
        _invalidatedShapes.Add(shape);
    }

    internal void ClearInvalidation()
    {
        _invalidatedShapes.Clear();
    }

    internal void UpdateVisibility(bool canRender)
    {
        foreach (var shape in _shapes)
        {
            shape.CanRender = canRender;
        }
    }
}
