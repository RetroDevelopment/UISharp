using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A basic container that contains one object.
/// </summary>
public class Panel : UIContainer, ISingleContainer // TODO: ISingleContainer should have a property not SetComponent() and GetChildren does not apply
{
    private readonly Rectangle _backgroundRectangle;
    private UIWidget? _child;

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.Count() == 2 ? childrenSize.ElementAt(1) : Size.Zero;

    /// <inheritdoc />

    public override IEnumerable<UIWidget> Children => [GetChildrenNodes().ElementAt(1)];

    /// <summary>
    /// Creates a new panel.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public Panel(Application application) : base(application)
    {
        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        AddChildNode(_backgroundRectangle);
    }

    /// <summary>
    /// Sets the component to be inserted in <see langword="this" /> panel.
    /// </summary>
    /// <param name="component">The component to be inserted in <see langword="this" /> panel.</param>
    public void SetComponent(UIWidget component)
    {
        if (_child != null) RemoveChildNode(_child);
        _child = component;
        AddChildNode(_child);
    }
}
