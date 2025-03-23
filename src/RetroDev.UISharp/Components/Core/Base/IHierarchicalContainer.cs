using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// All hierarchical containers must implement this interface.
/// </summary>
public interface IHierarchicalContainer
{
    /// <summary>
    /// The list of logical children of a container. Note that this differs from <see cref="UIComponent.Children"/> which is the
    /// list of actual children in the UI tree view.
    /// </summary>
    /// <example>
    /// A layout <see cref="UIComponent.Children"/> may be a list of panels containing wrapping labels plus additional decorator components.
    /// A layout <see cref="Items"/> is the list of labels, which are the actual children participating in the layout.
    /// </example>
    /// <remarks>
    /// The distinction between actual children and logical children is made to abstract away from implementation details and from those UI children which
    /// must not be publicly exposed.
    /// </remarks>
    UIPropertyCollection<UINode> Items { get; }
}
