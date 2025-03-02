using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Layout;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Base;

/// <summary>
/// Represents a widget that is part of the UI tree but it is not a root.
/// For example, it can be a label, button or a layout, but not a window (or a <see cref="UIRoot"/>).
/// </summary>
public abstract class UIWidget : UIComponent
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
    protected UIWidget(Application application, ComponentVisibility visibility = ComponentVisibility.Visible, bool isFocusable = true, IAutoSize? autoWidth = null, IAutoSize? autoHeight = null, IHorizontalAlignment? horizontalAlignment = null, IVerticalAlignment? verticalAlignment = null) : base(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment)
    {
    }
}
