using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;

namespace RetroDev.UISharp.Components.Core;

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
    public void Render(UIComponent root, IRenderingEngine renderingEngine)
    {
        if (!_invalidator.HasInvalidatedNodes) return;

        renderingEngine.InitializeFrame(root.BackgroundColor.Value);

        var component = _invalidator.GetNextInvalidatedComponent();
        while (component != null)
        {
            component.OnRenderFrame();
            component.CancelInvalidation();
            component = _invalidator.GetNextInvalidatedComponent();
        }

        renderingEngine.FinalizeFrame();
        _invalidator.Reset();
    }
}
