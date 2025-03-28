using System;
using RetroDev.UISharp.UIDefinition.Ast;

namespace RetroDev.UISharp.Presentation.Properties.Binding;

internal class UIHierarchyFlattenBinder<TSource, TDestination> : IDisposable
{
    private readonly UIPropertyHierarchy<TSource> _shourceHierarchy;
    private readonly UIPropertyCollection<TDestination> _destinationCollection;
    private readonly IBindingValueConverter<UITreeNode<TSource>, TDestination> _converter;
    private readonly Dictionary<UIPropertyHierarchy<TSource>, TDestination> _treeToListMapping = [];
    private readonly Dictionary<UIPropertyHierarchy<TSource>, List<IDisposable>> _subscriptions = [];

    public UIHierarchyFlattenBinder(UIPropertyHierarchy<TSource> sourceHierarchy,
                                    UIPropertyCollection<TDestination> destinationCollection,
                                    IBindingValueConverter<UITreeNode<TSource>, TDestination> converter)
    {
        _shourceHierarchy = sourceHierarchy;
        _destinationCollection = destinationCollection;
        _converter = converter;

        _destinationCollection.IsReadOnly = true;

        SubscribeNode(_shourceHierarchy);
        using (destinationCollection.CreateBindingScope())
        {
            destinationCollection.AddRange(sourceHierarchy.Children.SelectMany(Flatten));
        }
    }

    /// <summary>
    /// Maps the given <paramref name="node"/> to the respective value in the flatten list.
    /// </summary>
    /// <param name="node">The node to map.</param>
    /// <returns>The mapped node in the flatten list.</returns>
    public TDestination Map(UITreeNode<TSource> node) =>
        _treeToListMapping[node];

    public void Dispose()
    {
        _subscriptions
            .SelectMany(subscription => subscription.Value)
            .ToList()
            .ForEach(subcription => subcription.Dispose());

        _subscriptions.Clear();
        GC.SuppressFinalize(this);
    }

    private void OnNodeAdd(int index, UIPropertyHierarchy<TSource> root)
    {
        using var _ = _destinationCollection.CreateBindingScope();
        var node = root.Children[index];
        var flattenList = Flatten(node);
        var previousNode = index == 0 ? root : root.Children[index - 1];
        var previousNodeInList = _treeToListMapping[previousNode];
        var previousNodeListIndex = _destinationCollection.IndexOf(previousNodeInList);
        if (previousNodeListIndex == _destinationCollection.Count - 1)
        {
            _destinationCollection.AddRange(flattenList);
        }
        else
        {
            _destinationCollection.InsertRange(previousNodeListIndex + 1, flattenList);
        }
    }

    private void OnNodeRemove(int index, UIPropertyHierarchy<TSource> root)
    {
        using var _ = _destinationCollection.CreateBindingScope();
        var node = root.Children[index];
        var nodeInList = _treeToListMapping[node];
        var nodeInListIndex = _destinationCollection.IndexOf(nodeInList);
        var count = Remove(node);
        _destinationCollection.RemoveRange(nodeInListIndex, count);
    }

    private List<TDestination> Flatten(UITreeNode<TSource> node)
    {
        SubscribeNode(node);
        var destination = _converter.ConvertSourceToDestination(node);
        _treeToListMapping[node] = destination;
        List<TDestination> result = [destination];
        foreach (var child in node.Children)
        {
            result.AddRange(Flatten(child));
        }

        return result;
    }

    private int Remove(UITreeNode<TSource> node)
    {
        _subscriptions[node].ForEach(subscription => subscription.Dispose());
        _subscriptions.Remove(node);
        _treeToListMapping.Remove(node);
        var count = 1;

        foreach (var child in node.Children)
        {
            Remove(child);
            count += 1;
        }

        return count;
    }

    private void SubscribeNode(UIPropertyHierarchy<TSource> node)
    {
        _subscriptions.Add(node, []);

        _subscriptions[node].Add(node
            .Children
            .ValueAdd
            .Subscribe(index => OnNodeAdd(index, node)));

        _subscriptions[node].Add(node
            .Children
            .ValueRemove
            .Subscribe(index => OnNodeRemove(index, node)));
    }
}
