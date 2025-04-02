using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Components.Containers;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// All containers implementing a single child must implement this interface.
/// </summary>
public interface ISingleContainer
{
    /// <summary>
    /// The container logical child.
    /// </summary>
    /// <remarks>
    /// A logical child differs from an actual child stored in <see cref="UIObject.Children"/>. The former is an element publicly exposed that is contained
    /// by a container, the latter is an actual child in the UI tree.
    /// For example a <see cref="ScrollView"/> has scroll bars and the component to scroll as actual children (<see cref="UIObject.Children"/>) but
    /// only the component to scroll is a logical child (<see cref="Item"/>).
    /// The distinction between actual children and logical children is made to abstract away from implementation details and from those UI children which
    /// must not be publicly exposed.
    /// </remarks>
    UIProperty<UIControl?> Item { get; }
}
