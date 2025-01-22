using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Events.Internal;
using RetroDev.OpenUI.Graphics;

namespace RetroDev.OpenUI.Components.RetainedRendering;

/// <summary>
/// Iterates over a <see cref="UIComponent"/> hierarchy to call <see cref="UIComponent.OnRenderFrame(RetroDev.OpenUI.Events.RenderingEventArgs)"/>
/// for each component to redraw, but it only draws elements that need to be redrawn.
/// </summary>
internal class RetaineModeCanvas
{
    /// <summary>
    /// The retained mode rendering entry point. It renders a frame in retain-mode, meaning that it only
    /// renders components that need a redraw.
    /// </summary>
    /// <param name="root">The root component to render, usually a <see cref="Window"/>.</param>
    public void Render(UIComponent root, Canvas canvas, IRenderingEngine renderingEngine)
    {
        var renderingEventArgs = new RenderingEventArgs(canvas);
        var componentsToRender = root.GetComponentTreeNodesDepthFirstSearch()
                                     .Where(WillBeRendered);

        foreach (var component in componentsToRender)
        {
            component.OnRenderFrame(renderingEventArgs);
        }
    }

    private bool WillBeRendered(UIComponent component)
    {
        if (component.Visibility != ComponentVisibility.Visible) return false;
        var drawingArea = component.AbsoluteDrawingArea;
        if (drawingArea.TopLeft.X > 800 || drawingArea.TopLeft.Y > 600) return false;
        if (drawingArea.BottomRight.X < 0 || drawingArea.BottomRight.Y < 0) return false;
        var clippedSize = drawingArea.Clip(component.ClipArea).Size;
        if (clippedSize.Width == 0 || clippedSize.Height == 0) return false;
        return true;
    }
}
