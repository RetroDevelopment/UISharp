using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties;

internal class UITreeNodeRecursiveConverter<TSource, TDestination> : IBindingValueConverter<UITreeNode<TSource>, UITreeNode<TDestination>>
{
    private readonly IBindingValueConverter<UITreeNode<TSource>, UITreeNode<TDestination>> _converter;
    private readonly BindingType _bindingType;

    public UITreeNodeRecursiveConverter(IBindingValueConverter<UITreeNode<TSource>, UITreeNode<TDestination>> converter, BindingType bindingType)
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
}
