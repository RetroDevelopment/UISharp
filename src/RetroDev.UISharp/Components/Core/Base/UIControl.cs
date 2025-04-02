using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// Represents a widget that is part of the UI tree but it is not a root.
/// For example, it can be a label, button or a layout, but not a window (or a <see cref="UISurface"/>).
/// </summary>
/// <remarks>
/// When implementing a new <see cref="UIControl"/> you should consider the following:
/// - Position the content within the rendering area taking into account padding. <br />
/// - If implementing a layout or container, you can wrap each child into a <see cref="Panel"/> which will automatically take each child margin into account when computing the wrap size.<br />
/// - Margin calculation for this component will be performed automatically. <br />
/// - Padding calculation in <see cref="UIObject.ComputeMinimumOptimalSize(IEnumerable{UISharp.Core.Coordinates.Size})"/> implementation must be disregarded. <br />
/// In summary: consider only padding of this component (children margin is already taken into account by <see cref="Panel"/>), and leave every other margin/padding calculation to the framework.
/// </remarks>
public abstract class UIControl : UIElement
{
    /// <summary>
    /// Creates a new UI node.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="visibility">Whether the component is rendered or not.</param>
    /// <param name="isFocusable">Whether the component can get focus.</param>
    /// <param name="autoWidth">How to automatically determine this component width.</param>
    /// <param name="autoHeight">How to automatically determine this component height.</param>
    /// <param name="horizontalAlignment">The component horizontal alignment (relative to its <see cref="Parent"/>).</param>
    /// <param name="verticalAlignment">The component vertical alignment (relative to its <see cref="Parent"/>).</param>
    protected UIControl(Application application, ComponentVisibility visibility = ComponentVisibility.Visible, bool isFocusable = true, IAutoSize? autoWidth = null, IAutoSize? autoHeight = null, IHorizontalAlignment? horizontalAlignment = null, IVerticalAlignment? verticalAlignment = null) : base(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment)
    {
    }
}
