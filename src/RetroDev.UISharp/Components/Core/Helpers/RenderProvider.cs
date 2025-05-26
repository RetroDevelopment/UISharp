using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Windows;

namespace RetroDev.UISharp.Components.Core.Helpers;

/// <summary>
/// Iterates over a <see cref="UIObject"/> hierarchy to call <see cref="UIObject.OnRenderFrame(RenderingEventArgs)"/>
/// for each component to redraw, but it only draws elements that need to be redrawn.
/// </summary>
/// <param name="invalidator">The invalidator that contains the list of invalidated <see cref="UIObject"/> to render.</param>
public class RenderProvider(Invalidator invalidator)
{
    private readonly Invalidator _invalidator = invalidator;

    /// <summary>
    /// The retained mode rendering entry point. It renders a frame in retain-mode, meaning that it only
    /// renders components that need a redraw.
    /// </summary>
    /// <param name="root">The root component to render, usually a <see cref="Window"/>.</param>
    public void Render(UIObject root, IRenderingEngine renderingEngine)
    {
        if (!_invalidator.HasInvalidatedNodes) return;

        renderingEngine.InitializeFrame(root.BackgroundColor.Value);

        var component = _invalidator.GetNextInvalidatedComponent();
        while (component is not null)
        {
            component.OnRenderFrame();
            component.CancelInvalidation();
            component = _invalidator.GetNextInvalidatedComponent();
        }

        renderingEngine.FinalizeFrame();
        _invalidator.Reset();
    }
}
