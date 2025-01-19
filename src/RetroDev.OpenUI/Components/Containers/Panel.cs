using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A basic container that contains one object.
/// </summary>
public class Panel : Container, ISingleContainer
{
    private UIComponent? _child;

    /// <inheritdoc />
    protected override Size ComputeSizeHint() => new(100, 100);

    /// <inheritdoc />

    public override IEnumerable<UIComponent> Children => GetChildren();

    /// <summary>
    /// Creates a new panel.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public Panel(Application application) : base(application)
    {
    }

    /// <summary>
    /// Sets the component to be inserted in <see langword="this" /> panel.
    /// </summary>
    /// <param name="component">The component to be inserted in <see langword="this" /> panel.</param>
    public void SetComponent(UIComponent component)
    {
        if (_child != null) RemoveChild(_child);
        _child = component;
        AddChild(_child);
    }
}
