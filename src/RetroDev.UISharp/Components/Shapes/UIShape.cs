using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Coordinates;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Shapes;

/// <summary>
/// A shape to render in the <see cref="Canvas"/>.
/// </summary>
/// <remarks>
/// Shapes can be modified at any time. Since retained mode is used, only when a shape is modified
/// it will be effectively re-drawn.
/// </remarks>
public abstract class UIShape
{
    /// <summary>
    /// The drawing area relative to the canvas.
    /// </summary>
    public ShapeProperty<UIShape, Area> RelativeRenderingArea { get; }

    // TODO: include RelativeClipArea property so that it is possible to override the absolute clipping area (useful for progress bar with rounded corners)

    /// <summary>
    /// The shape background color.
    /// </summary>
    public ShapeProperty<UIShape, Color> BackgroundColor { get; }

    /// <summary>
    /// Whether the shape is visible.
    /// </summary>
    /// <remarks>
    /// When <see cref="Canvas.Component"/> is not visible, <see langword="this" /> <see cref="UIShape"/> will not be visible
    /// even if <see cref="Visible"/> is <see langword="true" />.
    /// </remarks>
    public ShapeProperty<UIShape, bool> Visible { get; }

    /// <summary>
    /// The engine rendering this shape.
    /// </summary>
    protected IRenderingEngine? RenderingEngine => Canvas?.RenderingEngine;

    /// <summary>
    /// The actual graphical element to render. This is the object instance that directly
    /// updates the memory in the <see cref="IRenderingEngine"/> used to render this shape.
    /// Changes made to this element may directly trigger redraw, so only change it in the <see cref="Render"/> method.
    /// </summary>
    protected abstract RenderingElement RenderingElement { get; }

    /// <summary>
    /// The rendering order. The shape with index 0 is rendered first, then the other shapes will be rendered in ascending order.
    /// This property value must be unique for each shape.
    /// </summary>
    internal ShapeProperty<UIShape, uint> ZIndex { get; }

    /// <summary>
    /// The canvas area owning <see langword="this" /> <see cref="UIShape"/>.
    /// </summary>
    internal Canvas? Canvas { get; set; }

    /// <summary>
    /// Whether <see langword="this" /> shape can be rendered.
    /// It is <see langword="true" /> if an only if <see cref="Canvas.Component"/> and all its parents
    /// are visible, otherwise it is <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// This flag is used as a visibility override. If false, this shape will always be invisible even if
    /// <see cref="Visible"/> is <see langword="true" />.
    /// </remarks>
    internal bool CanRender { get; set; }

    /// <summary>
    /// Creates a new shape.
    /// </summary>
    /// <param name="application">The running application.</param>
    protected UIShape(Application application)
    {
        RelativeRenderingArea = new ShapeProperty<UIShape, Area>(this, application, Area.Empty);
        BackgroundColor = new ShapeProperty<UIShape, Color>(this, application, Color.Transparent);
        Visible = new ShapeProperty<UIShape, bool>(this, application, true);
        ZIndex = new ShapeProperty<UIShape, uint>(this, application, 0);
    }

    /// <summary>
    /// Invalidates <see langword="this" /> shape meaning that it needs to be re-drawn.
    /// </summary>
    public void Invalidate()
    {
        Canvas?.AddInvalidatedShape(this);
    }

    /// <summary>
    /// Add the shape to the <see cref="RenderingEngine"/>.
    /// </summary>
    protected internal abstract void AddShapeToRenderingEngine();

    /// <summary>
    /// Remove the shape to the <see cref="RenderingEngine"/>.
    /// </summary>
    protected internal abstract void RemoveShapeFromRenderingEngine();

    /// <summary>
    /// Renders the shape.
    /// </summary>
    protected internal abstract void Render();

    internal void Render(Area clipArea)
    {
        if (Canvas == null) throw new InvalidOperationException("Canvas must be set when rendering.");

        RenderingElement.Visible = CanRender && Visible.Value;
        RenderingElement.RenderingArea = RelativeRenderingArea.Value.ToAbsolute(Canvas.ContainerAbsoluteDrawingArea);
        RenderingElement.ClipArea = clipArea;
        RenderingElement.BackgroundColor = BackgroundColor.Value;
        RenderingElement.ZIndex = RenderingElement.ConvertToInternalZIndex(ZIndex.Value);
        Render();
    }
}
