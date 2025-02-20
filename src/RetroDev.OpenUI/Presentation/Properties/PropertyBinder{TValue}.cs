namespace RetroDev.OpenUI.Presentation.Properties;

internal class PropertyBinder<TSourceValue, TDestinationValue> : IBinder
{
    private class LambdaConverter<T, S>(Func<T, S> sourceToDestinationConvert, Func<S, T> destinationToSourceConvert) : IBindingValueConverter<T, S>
    {
        public T Convert(S value) =>
            destinationToSourceConvert(value);

        public S Convert(T value) =>
            sourceToDestinationConvert(value);
    }

    private readonly BindableProperty<TSourceValue> _sourceProperty;
    private readonly BindableProperty<TDestinationValue> _destinationProperty;
    private readonly IBindingValueConverter<TSourceValue, TDestinationValue> _converter;

    public PropertyBinder(BindableProperty<TSourceValue> sourceProperty,
                          BindableProperty<TDestinationValue> destinationProperty,
                          BindingType type,
                          IBindingValueConverter<TSourceValue, TDestinationValue> converter)
    {
        CheckValidBinding(sourceProperty, destinationProperty, type);

        _sourceProperty = sourceProperty;
        _destinationProperty = destinationProperty;
        _converter = converter;

        switch (type)
        {
            case BindingType.SourceToDestination:
                _sourceProperty.ValueChange += SourceProperty_ValueChange;
                _destinationProperty.Value = _converter.Convert(_sourceProperty.Value);
                break;
            case BindingType.DestinationToSource:
                _destinationProperty.ValueChange += DestinationProperty_ValueChange;
                _sourceProperty.Value = _converter.Convert(_destinationProperty.Value);
                break;
            case BindingType.TwoWays:
                _sourceProperty.ValueChange += SourceProperty_ValueChange;
                _destinationProperty.Value = _converter.Convert(_sourceProperty.Value);
                _destinationProperty.ValueChange += DestinationProperty_ValueChange;
                _sourceProperty.Value = _converter.Convert(_destinationProperty.Value);
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {type}");
        }
    }

    public PropertyBinder(BindableProperty<TSourceValue> sourceProperty,
                          BindableProperty<TDestinationValue> destinationProperty,
                          BindingType type,
                          Func<TSourceValue, TDestinationValue> sourceToDestinationConvert,
                          Func<TDestinationValue, TSourceValue> destinationToSourceConvert) : this(sourceProperty,
                                                                                               destinationProperty,
                                                                                               type,
                                                                                               new LambdaConverter<TSourceValue, TDestinationValue>(sourceToDestinationConvert, destinationToSourceConvert))
    {
    }

    public void Unbind()
    {
        _sourceProperty.ValueChange -= SourceProperty_ValueChange;
        _destinationProperty.ValueChange -= DestinationProperty_ValueChange;
    }

    private void SourceProperty_ValueChange(BindableProperty<TSourceValue> sender, ValueChangeEventArgs<TSourceValue> e)
    {
        _destinationProperty.Value = _converter.Convert(e.CurrentValue);
    }

    private void DestinationProperty_ValueChange(BindableProperty<TDestinationValue> sender, ValueChangeEventArgs<TDestinationValue> e)
    {
        _sourceProperty.Value = _converter.Convert(e.CurrentValue);
    }

    private void CheckValidBinding(BindableProperty<TSourceValue> sourceProperty, BindableProperty<TDestinationValue> destinationProperty, BindingType type)
    {
        switch (type)
        {
            case BindingType.SourceToDestination:
                if (sourceProperty.AllowedBinding != BindingType.SourceToDestination && sourceProperty.AllowedBinding != BindingType.TwoWays) throw new InvalidOperationException($"Invliad binding {sourceProperty} -> {destinationProperty}: source property does not allow for that binding.");
                if (destinationProperty.AllowedBinding != BindingType.DestinationToSource && destinationProperty.AllowedBinding != BindingType.TwoWays) throw new InvalidOperationException($"Invliad binding {sourceProperty} -> {destinationProperty}: destination property does not allow for that binding.");
                break;
            case BindingType.DestinationToSource:
                if (sourceProperty.AllowedBinding != BindingType.DestinationToSource && sourceProperty.AllowedBinding != BindingType.TwoWays) throw new InvalidOperationException($"Invliad binding {sourceProperty} <- {destinationProperty}: source property does not allow for that binding.");
                if (destinationProperty.AllowedBinding != BindingType.SourceToDestination && destinationProperty.AllowedBinding != BindingType.TwoWays) throw new InvalidOperationException($"Invliad binding {sourceProperty} <- {destinationProperty}: destination property does not allow for that binding.");
                break;
            case BindingType.TwoWays:
                if (sourceProperty.AllowedBinding != BindingType.TwoWays) throw new InvalidOperationException($"Invliad binding {sourceProperty} <-> {destinationProperty}: source property does not allow for two way binding.");
                if (destinationProperty.AllowedBinding != BindingType.TwoWays) throw new InvalidOperationException($"Invliad binding {sourceProperty} <-> {destinationProperty}: destination property does not allow for two way binding.");
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {type}");
        }
    }
}
