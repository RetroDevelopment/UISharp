namespace RetroDev.OpenUI.Components.Core;

using OpenUI.Properties;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;

/// <summary>
/// The class responsible of component invalidation logic of a <see cref="UIComponent"/>.
/// An component is invalidated if at least one <see cref="UIProperty{TParent, TValue}"/> value changes.
/// If a compoenent is invalidated it needs to be re-rendered and additional checks or positioning logic may be needed.
/// An <see cref="Invalidator"/> manages invalidation logic of one <see cref="Component"/>.
/// </summary>
public class Invalidator
{
    private void RegisterRenderingAreaChangeEvents()
    {
        // Component.X.ValueChange += (_, _) => OnAreaChange();
        // Component.Y.ValueChange += (_, _) => OnAreaChange();
        // Component.Width.ValueChange += (_, _) => OnAreaChange();
        // Component.Height.ValueChange += (_, _) => OnAreaChange();
        // Component.AutoWidth.ValueChange += (_, _) => OnAreaChange();
        // Component.AutoHeight.ValueChange += (_, _) => OnAreaChange();
        // Component.HorizontalAlignment.ValueChange += (_, _) => OnAreaChange();
        // Component.VerticalAlignment.ValueChange += (_, _) => OnAreaChange();
        // Component.Visibility.ValueChange += (_, _) => OnAreaChange();
    }

    private void OnAreaChange()
    {
        // var args = new ValueChangeEventArgs<Area>(OriginalRelativeDrawingArea, Component.RelativeDrawingArea);
        // RenderingAreaChange.Invoke(Component, args);
    }
}
