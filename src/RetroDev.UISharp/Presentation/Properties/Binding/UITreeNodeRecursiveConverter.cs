using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties;

internal class UITreeNodeRecursiveConverter<TSource, TDestination> : IHierarchicalBindingValueConverter<TSource, TDestination>
{
    private readonly IHierarchicalBindingValueConverter<TSource, TDestination> _converter;
    private readonly BindingType _bindingType;

    public UITreeNodeRecursiveConverter(IHierarchicalBindingValueConverter<TSource, TDestination> converter, BindingType bindingType)
    {
        _converter = converter;
        _bindingType = bindingType;
    }

    public UITreeNode<TDestination> ConvertSourceToDestination(UITreeNode<TSource> source)
    {
        var destinationNode = _converter.ConvertSourceToDestination(source);
        destinationNode.Bind(source, _bindingType, _converter);
        return destinationNode;
    }

    public UITreeNode<TSource> ConvertDestinationToSource(UITreeNode<TDestination> destination)
    {
        var sourceNode = _converter.ConvertDestinationToSource(destination);
        sourceNode.Bind(destination, _bindingType, _converter.Flip());
        return sourceNode;
    }

    public TDestination ConvertSourceToDestination(TSource source) =>
        _converter.ConvertSourceToDestination(source);

    public TSource ConvertDestinationToSource(TDestination destination) =>
        _converter.ConvertDestinationToSource(destination);
}
