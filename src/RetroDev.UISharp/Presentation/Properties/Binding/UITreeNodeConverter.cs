using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp;

internal class UITreeNodeConverter<TSource, TDestination> : IBindingValueConverter<UITreeNode<TSource>, UITreeNode<TDestination>>
{
    private readonly IBindingValueConverter<TSource, TDestination> _converter;
    private readonly BindingType _bindingType;

    public UITreeNodeConverter(IBindingValueConverter<TSource, TDestination> converter, BindingType bindingType)
    {
        _converter = converter;
        _bindingType = bindingType;
    }

    public UITreeNode<TDestination> ConvertSourceToDestination(UITreeNode<TSource> source)
    {
        var destinationValue = _converter.ConvertSourceToDestination(source.Content.Value);
        var destinationNode = Create(source._application, source._component, destinationValue);
        destinationNode.Bind(source, _bindingType, _converter);
        return destinationNode;
    }

    public UITreeNode<TSource> ConvertDestinationToSource(UITreeNode<TDestination> destination)
    {
        var sourceValue = _converter.ConvertDestinationToSource(destination.Content.Value);
        var sourceNode = Create(destination._application, destination._component, sourceValue);
        sourceNode.Bind(destination, _bindingType, _converter.Flip());
        return sourceNode;
    }

    private static UITreeNode<T> Create<T>(Application? application, UIComponent? component, T value)
    {
        if (component != null)
        {
            return new UITreeNode<T>(component, value);
        }
        else if (application != null)
        {
            return new UITreeNode<T>(application, value);
        }
        else
        {
            throw new ArgumentException("Application and component cannot be both null");
        }
    }
}
