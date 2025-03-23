using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// Represents a node in a UI hierarchical view, that is a tree representation of a collection of components.
/// A node contains one and only one component and each children is a node containing one component each.
/// </summary>
public class UINode
{
    /// <summary>
    /// The <see cref="UIWidget"/> contained by <see langword="this" /> <see cref="UINode"/>.
    /// </summary>
    public UIProperty<UIWidget> Component { get; }

    /// <summary>
    /// The component children, which is an empty list if there are no children.
    /// </summary>
    public UIPropertyCollection<UINode> Children { get; }

    /// <summary>
    /// Creates a new <see cref="UINode"/>.
    /// </summary>
    /// <param name="component">The <see cref="UIWidget"/> contained by <see langword="this" /> <see cref="UINode"/>.</param>
    public UINode(UIWidget component)
    {
        Component = new UIProperty<UIWidget>(component.Application, component, lockSetter: true);
        Children = new UIPropertyCollection<UINode>(component.Application, lockChanges: true);
    }
}
