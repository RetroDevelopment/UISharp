using System.Diagnostics;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

public class UITreeProperty<TValue>
{
    public UIPropertyCollection<UITreeNode<TValue>> Nodes { get; }

    public UITreeProperty(Application application, bool lockChanges = false)
    {
        Nodes = new UIPropertyCollection<UITreeNode<TValue>>(application, lockChanges);
        RegisterChildrenChangeCallbakcs();
    }

    public UITreeProperty(UIComponent component, bool lockChanges = true)
    {
        Nodes = new UIPropertyCollection<UITreeNode<TValue>>(component, lockChanges);
        RegisterChildrenChangeCallbakcs();
    }

    public void Bind<TSource>(UITreeProperty<TSource> source, BindingType bindingType, IBindingValueConverter<TSource, TValue> converter)
    {
        Unbind();

        var nodeConverter = new UITreeNodeConverter<TSource, TValue>(converter, bindingType);

        Nodes.Bind(source.Nodes, bindingType, nodeConverter);

        if (Nodes.Count != source.Nodes.Count)
        {
            throw new UIPropertyValidationException($"Invalid tree binding: source and destination must have the same number of children (source has {source.Nodes.Count} children and destination has {Nodes.Count} children)");
        }

        var childrenCount = source.Nodes.Count;
        for (int i = 0; i < childrenCount; i++)
        {
            var sourceChild = source.Nodes[i];
            var destinationChild = Nodes[i];
            destinationChild.Bind(sourceChild, bindingType, converter);
        }
    }

    public void Unbind()
    {
        foreach (var child in Nodes) child.Unbind();
    }

    private void RegisterChildrenChangeCallbakcs()
    {
        Nodes.ValueAdd.Subscribe(OnChildAdd);
        Nodes.ValueRemove.Subscribe(OnChildRemove);
    }

    private void OnChildAdd(int index)
    {
        var child = Nodes[index];
        child.TreeLevel = 0;
        child.Parent?.Children?.Remove(child);
        child.Parent = null;
    }

    private void OnChildRemove(int index)
    {
        var child = Nodes[index];
        child.TreeLevel = 0;
        child.Parent = null;
    }

}
