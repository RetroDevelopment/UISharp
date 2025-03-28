using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties.Binding;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Represents a tree and provides hierarchical binding.
/// This is useful for hierarchical components displaying tree views and the respective bindings with
/// hierarchical objects (e.g. a directory tree).
/// </summary>
/// <typeparam name="TValue">The node content value type.</typeparam>
public class UIPropertyHierarchy<TValue>
{
    private IDisposable? _flatBinder;
    internal readonly Application? _application;
    internal readonly UIComponent? _component;

    /// <summary>
    /// The node direct children.
    /// </summary>
    public UIPropertyCollection<UITreeNode<TValue>> Children { get; }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <param name="application">The application owning <see langword="this" /> node.</param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UINode{TValue}"/> during event handling.</param>
    public UIPropertyHierarchy(Application application, bool lockSetter = false)
    {
        _application = application;
        Children = new UIPropertyCollection<UITreeNode<TValue>>(application, lockSetter);
        RegisterChildrenChangeCallbakcs();
    }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <param name="component">The component owning <see langword="this" /> node.</param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UINode{TValue}"/> during event handling.</param>
    public UIPropertyHierarchy(UIComponent component, bool lockSetter = true)
    {
        _component = component;
        Children = new UIPropertyCollection<UITreeNode<TValue>>(component, lockSetter);
        RegisterChildrenChangeCallbakcs();
    }

    /// <summary>
    /// Collapses <see langword="this" /> node and all its children.
    /// </summary>
    public virtual void CollapseAll()
    {
        foreach (var child in Children)
        {
            child.CollapseAll();
        }
    }

    /// <summary>
    /// Expands <see langword="this" /> node and all its children.
    /// </summary>
    public virtual void ExpandAll()
    {
        foreach (var child in Children)
        {
            child.ExpandAll();
        }
    }

    /// <summary>
    /// Bind the given <paramref name="sourceProperty"/> tree to <see langword="this" /> tree.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> nodes content value type.</typeparam>
    /// <param name="sourceProperty">The binding source.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the binding source property.
    /// </param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public virtual void Bind<TSource>(UIPropertyHierarchy<TSource> sourceProperty, BindingType bindingType, IBindingValueConverter<TSource, TValue> converter)
    {
        Unbind();
        var nodeConverter = new UITreeNodeConverter<TSource, TValue>(converter, bindingType);
        Children.Bind(sourceProperty.Children, bindingType, nodeConverter);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public virtual void Bind(UIPropertyHierarchy<TValue> sourceProperty, BindingType bindingType)
    {
        Bind(sourceProperty, bindingType, ValueConverterFactory.Identity<TValue>());
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public virtual void Bind<TSource>(UIPropertyHierarchy<TSource> sourceProperty,
                                      BindingType bindingType,
                                      Func<TSource, TValue> sourceToDestinationConverter,
                                      Func<TValue, TSource> destinationToSourceConverter)
    {
        Bind(sourceProperty, bindingType, ValueConverterFactory.FromLambda(sourceToDestinationConverter, destinationToSourceConverter));
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public virtual void BindDestinationToSource(UIPropertyHierarchy<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.DestinationToSource);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to source property value.</param>
    public virtual void BindDestinationToSource<TSource>(UIPropertyHierarchy<TSource> sourceProperty,
                                                         Func<TValue, TSource> sourceToDestinationConverter)
    {
        Bind(sourceProperty, BindingType.DestinationToSource, _ => throw new InvalidOperationException(), sourceToDestinationConverter);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public virtual void BindSourceToDestination(UIPropertyHierarchy<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.SourceToDestination);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to source property value.</param>
    public virtual void BindSourceToDestination<TSource>(UIPropertyHierarchy<TSource> sourceProperty,
                                                         Func<TSource, TValue> sourceToDestinationConverter)
    {
        Bind(sourceProperty, BindingType.SourceToDestination, ValueConverterFactory.FromLambda(sourceToDestinationConverter));
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the destination property.
    /// </param>
    public virtual void BindTwoWays(UIPropertyHierarchy<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.TwoWays);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The destination property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public virtual void BindTwoWays<TSource>(UIPropertyHierarchy<TSource> sourceProperty,
                                              Func<TSource, TValue> sourceToDestinationConverter,
                                              Func<TValue, TSource> destinationToSourceConverter)
    {
        Bind(sourceProperty, BindingType.TwoWays, sourceToDestinationConverter, destinationToSourceConverter);
    }

    /// <summary>
    /// Binds <see langword="this" /> <see cref="UIPropertyHierarchy{TValue}"/> to the given <paramref name="sourceCollection"/> by flattening a tree into a list
    /// using DFS visit (Depth First Search).
    /// </summary>
    /// <typeparam name="TSource">The collection value type.</typeparam>
    /// <param name="sourceCollection">The source collection in which to flatten <see langword="this" /> <see cref="UIPropertyHierarchy{TValue}"/>.</param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public virtual void FlatBindDestinationToSource<TSource>(UIPropertyCollection<TSource> sourceCollection, IBindingValueConverter<UITreeNode<TValue>, TSource> converter)
    {
        Unbind();
        _flatBinder = new UIHierarchyFlattenBinder<TValue, TSource>(this, sourceCollection, converter);
    }

    /// <summary>
    /// Binds <see langword="this" /> <see cref="UIPropertyHierarchy{TValue}"/> to the given <paramref name="sourceCollection"/> by flattening a tree into a list
    /// using DFS visit (Depth First Search).
    /// </summary>
    /// <typeparam name="TSource">The collection value type.</typeparam>
    /// <param name="sourceCollection">The source collection in which to flatten <see langword="this" /> <see cref="UIPropertyHierarchy{TValue}"/>.</param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public virtual void FlatBindDestinationToSource<TSource>(UIPropertyCollection<TSource> sourceCollection, Func<UITreeNode<TValue>, TSource> converter)
    {
        FlatBindDestinationToSource(sourceCollection, ValueConverterFactory.FromLambda(sourceToDestination: converter));
    }

    /// <summary>
    /// Removes a binding if any.
    /// </summary>
    /// <remarks>
    /// This method performs deep unbinding in tree nodes, meaning that it will remove
    /// all binding recursively made in the tree node descendants.
    /// However, this method does NOT perform deep unbinding for node content (<see cref="UINode.Component"/>) but only shallow unbinding.
    /// This means that it does NOT remove nested binding of properties made inside the <see cref="UINode.Component"/> object.
    /// </remarks>
    public virtual void Unbind()
    {
        _flatBinder?.Dispose();
        Children.Unbind();
        foreach (var child in Children) child.Unbind();
    }

    /// <summary>
    /// Triggered when adding a child to this hierarchy.
    /// </summary>
    /// <param name="index">The index of the added node in the children list.</param>
    protected virtual void OnChildAdd(int index)
    {
        var child = Children[index];
        child.TreeLevel = 0;
        child.Parent?.Children?.Remove(child);
        child.Parent = null;
    }

    /// <summary>
    /// Triggered when removing a child from this hierarchy.
    /// </summary>
    /// <param name="index">The index of the removed node in the children list.</param>
    protected virtual void OnChildRemove(int index)
    {
        var child = Children[index];
        child.TreeLevel = 0;
        child.Parent = null;
    }

    private void RegisterChildrenChangeCallbakcs()
    {
        Children.ValueAdd.Subscribe(OnChildAdd);
        Children.ValueRemove.Subscribe(OnChildRemove);
    }
}
