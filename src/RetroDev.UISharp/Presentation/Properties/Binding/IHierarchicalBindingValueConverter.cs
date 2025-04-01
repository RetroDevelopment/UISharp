namespace RetroDev.UISharp.Presentation.Properties.Binding;

/// <summary>
/// This class converts values of type <typeparamref name="TSource"/> into values of type <typeparamref name="TDestination"/>
/// and vice-versa for a hierarchical property. It is used to bind values of different types in a hierarchical property.
/// </summary>
/// <typeparam name="TSource">Any type representing the source property value type.</typeparam>
/// <typeparam name="TDestination">Any type representing the destination property value type.</typeparam>
/// <remarks>
/// When using <see cref="UIPropertyHierarchy{TValue}"/> it is not required to perform any bindings inside a converter, as all
/// recursive bindings are performed automatically. The only bindings that are necessary are possible deep bindings inside <typeparamref name="TSource"/> or <typeparamref name="TDestination"/> values.
/// For example, if binding a view model to a UI layout or container, it might be that the view model has a nested <see cref="UIProperty{TValue}"/> to be bound with a specific
/// inner UI component of the UI container or layout (e.g. a complex tree box where each node has a label and an edit box, and binding is needed from the edit box and the respective string in the view model).
/// </remarks>
public interface IHierarchicalBindingValueConverter<TSource, TDestination> : IBindingValueConverter<TSource, TDestination>, IBindingValueConverter<UITreeNode<TSource>, UITreeNode<TDestination>>
{
}
