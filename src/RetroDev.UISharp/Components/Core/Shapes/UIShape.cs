using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Shapes;

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
    public UIProperty<Area> RelativeRenderingArea { get; }

    /// <summary>
    /// Defines a custom clip area relative to the <see cref="Canvas.Component"/>.
    /// Note that the clip area will only restrict the shape boundaries, never extend.
    /// In other words, this property does not allow to render outside of the <see cref="Canvas.Component"/> region,
    /// but it allows to restrict its clip area.
    /// </summary>
    public UIProperty<Area?> ClipArea { get; }

    // TODO: include RelativeClipArea property so that it is possible to override the absolute clipping area (useful for progress bar with rounded corners)

    /// <summary>
    /// The shape background color.
    /// </summary>
    public UIProperty<Color> BackgroundColor { get; }

    /// <summary>
    /// Whether the shape is visible.
    /// </summary>
    /// <remarks>
    /// When <see cref="Canvas.Component"/> is not visible, <see langword="this" /> <see cref="UIShape"/> will not be visible
    /// even if <see cref="Visible"/> is <see langword="true" />.
    /// </remarks>
    public UIProperty<bool> Visible { get; }

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
    internal UIProperty<uint> ZIndex { get; }

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
        RelativeRenderingArea = new UIProperty<Area>(this, application, Area.Empty);
        ClipArea = new UIProperty<Area?>(this, application, (Area?)null);
        BackgroundColor = new UIProperty<Color>(this, application, Color.Transparent);
        Visible = new UIProperty<bool>(this, application, true);
        ZIndex = new UIProperty<uint>(this, application, 0);
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

        var customClipArea = ClipArea.Value?.ToAbsolute(Canvas.ContainerAbsoluteDrawingArea);
        RenderingElement.Visible = CanRender && Visible.Value;
        RenderingElement.RenderingArea = RelativeRenderingArea.Value.ToAbsolute(Canvas.ContainerAbsoluteDrawingArea);
        RenderingElement.ClipArea = customClipArea is not null ? customClipArea.Clip(clipArea) : clipArea;
        RenderingElement.BackgroundColor = BackgroundColor.Value;
        RenderingElement.ZIndex = ZIndex.Value;
        Render();
    }
}
