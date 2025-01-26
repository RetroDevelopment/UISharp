using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A basic container that contains one object.
/// </summary>
public class Panel : Container, ISingleContainer // TODO: ISingleContainer should have a property not SetComponent() and GetChildren does not apply
{
    private readonly Rectangle _backgroundRectangle;
    private UIComponent? _child;

    /// <inheritdoc />
    protected override Size ComputeSizeHint() => new(100, 100);

    /// <inheritdoc />

    public override IEnumerable<UIComponent> Children => [GetChildren().ElementAt(1)];

    /// <summary>
    /// Creates a new panel.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public Panel(Application application) : base(application)
    {
        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        AddChild(_backgroundRectangle);
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
