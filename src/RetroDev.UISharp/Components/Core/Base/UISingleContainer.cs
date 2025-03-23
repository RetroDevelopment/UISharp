using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Shapes;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// Abstract class for all containers containing one single element, which lay out multiple <see cref="UIComponent"/> instances.
/// </summary>
public abstract class UISingleContainer : UIWidget, ISingleContainer
{
    private readonly Rectangle _backgroundRectangle;

    /// <inheritdoc />
    public UIProperty<UIWidget?> Item { get; }

    /// <summary>
    /// The control border color.
    /// </summary>
    public UIProperty<Color> BorderColor { get; }

    /// <summary>
    /// Creates a new container.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="visibility">Whether the component is rendered or not.</param>
    /// <param name="isFocusable">Whether the component can get focus.</param>
    /// <param name="autoWidth">How to automatically determine this component width.</param>
    /// <param name="autoHeight">How to automatically determine this component height.</param>
    /// <param name="horizontalAlignment">The component horizontal alignment (relative to its <see cref="Parent"/>).</param>
    /// <param name="verticalAlignment">The component vertical alignment (relative to its <see cref="Parent"/>).</param>
    protected UISingleContainer(Application application,
                          ComponentVisibility visibility = ComponentVisibility.Visible,
                          bool isFocusable = false,
                          IAutoSize? autoWidth = null,
                          IAutoSize? autoHeight = null,
                          IHorizontalAlignment? horizontalAlignment = null,
                          IVerticalAlignment? verticalAlignment = null) : base(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment)
    {
        Item = new UIProperty<UIWidget?>(this, (UIWidget?)null);
        BorderColor = new UIProperty<Color>(this, Color.Transparent);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindSourceToDestination(BackgroundColor);
        _backgroundRectangle.BorderColor.BindSourceToDestination(BorderColor);
        _backgroundRectangle.BorderThickness.Value = 2.0f; // TODO: use styles
        Canvas.Shapes.Add(_backgroundRectangle);
        RenderFrame += UIContainer_RenderFrame;
    }

    private void UIContainer_RenderFrame(UIComponent sender, UISharp.Core.Windowing.Events.RenderingEventArgs e)
    {
        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
    }
}
