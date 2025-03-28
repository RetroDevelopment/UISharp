using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties.Binding;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Represents a tree node in a <see cref="UITreeProperty{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">The node content value type.</typeparam>
public class UITreeNode<TValue> : UIPropertyHierarchy<TValue>
{
    /// <summary>
    /// The node content.
    /// </summary>
    public UIProperty<TValue> Content { get; }

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
    public UITreeNode(Application application, TValue value, bool lockSetter = false) : base(application, lockSetter)
    {
        Content = new UIProperty<TValue>(application, value, lockSetter: lockSetter);
        Collapsed = new UIProperty<bool>(application, false, lockSetter: lockSetter);
    }

    /// <summary>
    /// Creates a new node.
    /// </summary>
    /// <param name="component">The component owning <see langword="this" /> node.</param>
    /// <param name="value">The node initial <see cref="Content"/> value.</param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UINode{TValue}"/> during event handling.</param>
    public UITreeNode(UIComponent component, TValue value, bool lockSetter = true) : base(component, lockSetter)
    {
        Content = new UIProperty<TValue>(component, value);
        Collapsed = new UIProperty<bool>(component, false);
    }

    /// <summary>
    /// Collapses <see langword="this" /> node and all its children.
    /// </summary>
    public override void CollapseAll()
    {
        Collapsed.Value = true;
        base.CollapseAll();
    }

    /// <summary>
    /// Expands <see langword="this" /> node and all its children.
    /// </summary>
    public override void ExpandAll()
    {
        Collapsed.Value = false;
        base.CollapseAll();
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
    public virtual void Bind<TSource>(UITreeNode<TSource> source, BindingType bindingType, IBindingValueConverter<TSource, TValue> converter)
    {
        base.Bind(source, bindingType, converter);
        Content.Bind(source.Content, bindingType, converter);
        Collapsed.Bind(source.Collapsed, bindingType);
    }

    /// <summary>
    /// Removes a binding if any.
    /// </summary>
    /// <remarks>
    /// This method performs deep unbinding in tree nodes, meaning that it will remove
    /// all binding recursively made in the tree node descendants.
    /// However, this method does NOT perform deep unbinding for node content (<see cref="Component"/>) but only shallow unbinding.
    /// This means that it does NOT remove nested binding of properties made inside the <see cref="Component"/> object.
    /// </remarks>
    public override void Unbind()
    {
        Content.Unbind();
        Collapsed.Unbind();
    }

    /// <summary>
    /// Triggered when adding a child to <see langword="this" /> node.
    /// </summary>
    /// <param name="index">The index of the added node in the children list.</param>
    protected override void OnChildAdd(int index)
    {
        base.OnChildAdd(index);
        var child = Children[index];
        UpdateTreeLevel(child, TreeLevel + 1);
        child.Parent = this;
    }

    private void UpdateTreeLevel(UITreeNode<TValue> node, int currentLevel)
    {
        node.TreeLevel = currentLevel;
        foreach (var child in node.Children)
        {
            UpdateTreeLevel(child, currentLevel + 1);
        }
    }
}
