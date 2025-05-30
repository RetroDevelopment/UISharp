﻿using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Shapes;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// Abstract class for all containers, which lay out multiple <see cref="UIObject"/> instances.
/// </summary>
public abstract class UIContainer : UIControl, IContainer
{
    private readonly Rectangle _backgroundRectangle;

    /// <inheritdoc />
    public UIPropertyCollection<UIControl> Items { get; }

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
    protected UIContainer(Application application,
                          ComponentVisibility visibility = ComponentVisibility.Visible,
                          bool isFocusable = false,
                          IAutoSize? autoWidth = null,
                          IAutoSize? autoHeight = null,
                          IHorizontalAlignment? horizontalAlignment = null,
                          IVerticalAlignment? verticalAlignment = null) : base(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment)
    {
        Items = new UIPropertyCollection<UIControl>(application);
        BorderColor = new UIProperty<Color>(this, Color.Transparent);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindSourceToDestination(BackgroundColor);
        _backgroundRectangle.BorderColor.BindSourceToDestination(BorderColor);
        _backgroundRectangle.BorderThickness.Value = 2.0f; // TODO: use styles
        Canvas.Shapes.Add(_backgroundRectangle);
        RenderFrame += UIContainer_RenderFrame;
    }

    /// <summary>
    /// Gets the child component with <see cref="ID"/> equal to the given <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="TComponent">The component type.</typeparam>
    /// <returns>The component.</returns>
    /// <exception cref="ArgumentException">If the component does not exist.</exception>
    /// <exception cref="InvalidCastException">If the component was found but with a type not assignable to <typeparamref name="TComponent"/>.</exception>
    public TComponent GetComponent<TComponent>(string id) where TComponent : UIControl
    {
        var children = Items.Where(c => c.ID.Value == id);
        if (!children.Any()) throw new ArgumentException($"Child with ID {id} not found in component with id {ID.Value}");
        return (TComponent)children.First();
    }

    private void UIContainer_RenderFrame(UIObject sender, UISharp.Core.Windowing.Events.RenderingEventArgs e)
    {
        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
    }
}
