using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Shapes;

/// <summary>
/// Allows to draw on a UI component surface.
/// </summary>
public class Canvas
{
    private readonly ILogger _logger;
    public readonly HashSet<UIShape> _invalidatedShapes = [];

    /// <summary>
    /// The list of shapes rendered in <see langword="this" /> <see cref="Canvas"/>.
    /// </summary>
    public UIPropertyCollection<UIShape> Shapes { get; }

    /// <summary>
    /// The <see cref="UIComponent"/> owning <see langword="this" /> <see cref="Canvas"/>.
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
        Shapes = new UIPropertyCollection<UIShape>(component.Application);
        Shapes.ValueAdd.Subscribe(OnShapeAdd);
        Shapes.ValueRemove.Subscribe(OnShapeRemove);
        _logger = component.Application.Logger;
    }

    internal void Attach(IRenderingEngine renderingEngine)
    {
        Detach();
        RenderingEngine = renderingEngine;
        foreach (var shape in Shapes)
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
        foreach (var shape in Shapes)
        {
            shape.CanRender = canRender;
        }
    }

    internal uint UpdateZIndices(uint baseZIndex)
    {
        var currentZIndex = baseZIndex;

        foreach (var shape in Shapes)
        {
            shape.ZIndex.Value = currentZIndex;
            currentZIndex++;
        }

        return currentZIndex;
    }

    private void OnShapeAdd(int index)
    {
        var shape = Shapes[index];
        _logger.LogVerbose($"Adding shape {shape}");
        shape.Canvas = this;
        shape.AddShapeToRenderingEngine();
        shape.Invalidate();
    }

    private void OnShapeRemove(int index)
    {
        var shape = Shapes[index];
        _logger.LogVerbose($"Removing shape {shape}");
        shape.Canvas = null;
        _invalidatedShapes.Remove(shape);
        shape.Canvas = null;
        shape.RemoveShapeFromRenderingEngine();
    }
}
