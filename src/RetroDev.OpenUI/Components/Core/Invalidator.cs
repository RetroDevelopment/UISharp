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
    private bool _invalidated = false;

    /// <summary>
    /// Event that is raised when the <see cref="Component"/> is invalidated.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, EventArgs> Invalidate = (_, _) => { };

    /// <summary>
    /// Event that is raised when the <see cref="Component"/> <see cref="UIComponent.RelativeDrawingArea"/> changes.
    /// </summary>
    public event TypeSafeEventHandler<UIComponent, ValueChangeEventArgs<Area>> RenderingAreaChange = (_, _) => { };

    /// <summary>
    /// The <see cref="UIComponent"/> for which to manage the invalidation logic.
    /// </summary>
    public UIComponent Component { get; }

    /// <summary>
    /// The relative <see cref="OriginalRelativeDrawingArea"/> value of <see cref="Component"/> before the component was
    /// invalidated.
    /// </summary>
    public Area OriginalRelativeDrawingArea { get; private set; }

    /// <summary>
    /// Whether <see cref="Component"/> is invalidated.
    /// </summary>
    public bool Invalidated
    {
        get
        {
            Component.Application.LifeCycle.ThrowIfNotOnUIThread();
            return _invalidated;
        }
        set
        {
            Component.Application.LifeCycle.ThrowIfNotOnUIThread();

            if (value)
            {
                // Cannot invalidate components during rendering phase.
                Component.Application.LifeCycle.ThrowIfPropertyCannotBeSet();
                if (!_invalidated)
                {
                    Invalidate.Invoke(Component, EventArgs.Empty);
                }
            }
            else
            {
                OriginalRelativeDrawingArea = Component.RelativeDrawingArea;
            }

            _invalidated = value;
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="Invalidator"/>.
    /// </summary>
    /// <param name="component">The <see cref="UIComponent"/> for which to manage invalidation logic.</param>
    public Invalidator(UIComponent component)
    {
        Component = component;
        RegisterRenderingAreaChangeEvents();
        OriginalRelativeDrawingArea = Area.Empty;
    }

    private void RegisterRenderingAreaChangeEvents()
    {
        Component.X.ValueChange += (_, _) => OnAreaChange();
        Component.Y.ValueChange += (_, _) => OnAreaChange();
        Component.Width.ValueChange += (_, _) => OnAreaChange();
        Component.Height.ValueChange += (_, _) => OnAreaChange();
        Component.AutoWidth.ValueChange += (_, _) => OnAreaChange();
        Component.AutoHeight.ValueChange += (_, _) => OnAreaChange();
        Component.HorizontalAlignment.ValueChange += (_, _) => OnAreaChange();
        Component.VerticalAlignment.ValueChange += (_, _) => OnAreaChange();
        Component.Visibility.ValueChange += (_, _) => OnAreaChange();
    }

    private void OnAreaChange()
    {
        var args = new ValueChangeEventArgs<Area>(OriginalRelativeDrawingArea, Component.RelativeDrawingArea);
        RenderingAreaChange.Invoke(Component, args);
    }
}
