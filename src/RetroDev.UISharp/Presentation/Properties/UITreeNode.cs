using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Represents a tree node in a <see cref="UITreeProperty{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The node content value type.</typeparam>
public class UITreeNode<TValue>
{
    internal readonly Application? _application;
    internal readonly UIComponent? _component;

    /// <summary>
    /// The node content.
    /// </summary>
    public UIProperty<TValue> Content { get; }

    /// <summary>
    /// The node direct children.
    /// </summary>
    public UIPropertyCollection<UITreeNode<TValue>> Children { get; }

    /// <summary>
    /// Whether the node is collapsed, meaning that all the children are not visible.
    /// </summary>
    public UIProperty<bool> Collapsed { get; }

    /// <summary>
    /// The node level in the tree. If a node is a root (<see cref="Parent"/> is <see langword="null" />) the level is 0.
    /// </summary>
    public int TreeLevel { internal set; get; } = 0;

    /// <summary>
    /// The node direct parent.
    /// </summary>
    public UITreeNode<TValue>? Parent { internal set; get; }

    /// <summary>
    /// Whether one or more ancestors are collapsed.
    /// If <see langword="this" /> node has no parent (<see cref="Parent"/> is <see langword="null" />)
    /// this property is <see langword="false" />.
    /// </summary>
    public bool AncestorCollapsed
    {
        get
        {
            if (Parent is null) return false;
            return Parent.Collapsed.Value || Parent.AncestorCollapsed;
        }
    }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <param name="application">The application owning <see langword="this" /> node.</param>
    /// <param name="value">The node initial <see cref="Content"/> value.</param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UINode{TValue}"/> during event handling.</param>
    public UITreeNode(Application application, TValue value, bool lockSetter = false)
    {
        _application = application;
        Content = new UIProperty<TValue>(application, value, lockSetter: lockSetter);
        Children = new UIPropertyCollection<UITreeNode<TValue>>(application, lockSetter);
        Collapsed = new UIProperty<bool>(application, false, lockSetter: lockSetter);
        RegisterChildrenChangeCallbakcs();
    }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <param name="component">The component owning <see langword="this" /> node.</param>
    /// <param name="value">The node initial <see cref="Content"/> value.</param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UINode{TValue}"/> during event handling.</param>
    public UITreeNode(UIComponent component, TValue value, bool lockSetter = true)
    {
        _component = component;
        Content = new UIProperty<TValue>(component, value);
        Children = new UIPropertyCollection<UITreeNode<TValue>>(component, lockSetter);
        Collapsed = new UIProperty<bool>(component, false);
        RegisterChildrenChangeCallbakcs();
    }

    /// <summary>
    /// Collapses <see langword="this" /> node and all its children.
    /// </summary>
    public void CollapseAll()
    {
        Collapsed.Value = true;
        foreach (var child in Children)
        {
            child.CollapseAll();
        }
    }

    /// <summary>
    /// Expands <see langword="this" /> node and all its children.
    /// </summary>
    public void ExpandAll()
    {
        Collapsed.Value = false;
        foreach (var child in Children)
        {
            child.ExpandAll();
        }
    }

    public void Bind<TSource>(UITreeNode<TSource> source, BindingType bindingType, IBindingValueConverter<TSource, TValue> converter)
    {
        Unbind();

        var nodeConverter = new UITreeNodeConverter<TSource, TValue>(converter, bindingType);

        Content.Bind(source.Content, bindingType, converter);
        Children.Bind(source.Children, bindingType, nodeConverter);
        Collapsed.Bind(source.Collapsed, bindingType);

        if (Children.Count == 0) return;

        if (Children.Count != source.Children.Count)
        {
            throw new UIPropertyValidationException($"Invalid tree binding: source and destination must have the same number of children (source has {source.Children.Count} children and destination has {Children.Count} children)");
        }

        var childrenCount = source.Children.Count;
        for (int i = 0; i < childrenCount; i++)
        {
            var sourceChild = source.Children[i];
            var destinationChild = Children[i];
            destinationChild.Bind(sourceChild, bindingType, converter);
        }
    }

    public void Unbind()
    {
        Content.Unbind();
        Children.Unbind();
        Collapsed.Unbind();
        foreach (var child in Children) child.Unbind();
    }

    private void RegisterChildrenChangeCallbakcs()
    {
        Children.ValueAdd.Subscribe(OnChildAdd);
        Children.ValueRemove.Subscribe(OnChildRemove);
    }

    private void OnChildAdd(int index)
    {
        var child = Children[index];
        child.TreeLevel = TreeLevel + 1;
        child.Parent?.Children?.Remove(child);
        child.Parent = this;
    }

    private void OnChildRemove(int index)
    {
        var child = Children[index];
        child.TreeLevel = 0;
        child.Parent = null;
    }
}
