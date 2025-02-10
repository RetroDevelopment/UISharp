using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.UI;

namespace RetroDev.OpenUI.Components.Core;

/// <summary>
/// Iterates over a <see cref="UIComponent"/> hierarchy to call <see cref="UIComponent.OnRenderFrame(RenderingEventArgs)"/>
/// for each component to redraw, but it only draws elements that need to be redrawn.
/// </summary>
/// <param name="invalidator">The invalidator that contains the list of invalidated <see cref="UIComponent"/> to render.</param>
public class RenderProvider(Invalidator invalidator)
{
    private readonly Invalidator _invalidator = invalidator;

    /// <summary>
    /// The retained mode rendering entry point. It renders a frame in retain-mode, meaning that it only
    /// renders components that need a redraw.
    /// </summary>
    /// <param name="root">The root component to render, usually a <see cref="Window"/>.</param>
    public void Render(UIComponent root, Canvas canvas, IRenderingEngine renderingEngine)
    {
        renderingEngine.InitializeFrame(root.BackgroundColor.Value);

        // TODO: when implementing retain mode via opengl render instancing (the following is pseudocode)
        // var component = _invalidator.GetNextInvalidatedComponent();
        // while (component != null)
        // {
        //     renderingEngine.UpdateShapes(component.Shapes); // Updates shapes instance attributes
        //     component.CancelInvalidation();
        //     component = _invalidator.GetNextInvalidatedComponent();
        // }
        // renderingEngine.Flush(); // 1 instanced draw call for each VBO (3 now, rectangles, circles, and text)

        var renderingEventArgs = new RenderingEventArgs(canvas);
        root.OnRenderFrame(renderingEventArgs);
        _invalidator.Reset();
    }
}
