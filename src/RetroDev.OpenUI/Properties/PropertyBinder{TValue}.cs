namespace RetroDev.OpenUI.Properties;

internal class PropertyBinder<TValue, TSourceParent, TDestinationParent> : IBinder
{
    private readonly BindableProperty<TSourceParent, TValue> _sourceProperty;
    private readonly BindableProperty<TDestinationParent, TValue> _destinationProperty;

    public PropertyBinder(BindableProperty<TSourceParent, TValue> sourceProperty, BindableProperty<TDestinationParent, TValue> destinationProperty, BindingType type)
    {
        CheckValidBinding(sourceProperty, destinationProperty, type);

        _sourceProperty = sourceProperty;
        _destinationProperty = destinationProperty;

        switch (type)
        {
            case BindingType.SourceToDestination:
                _sourceProperty.ValueChange += SourceProperty_ValueChange;
                _destinationProperty.Value = _sourceProperty.Value;
                break;
            case BindingType.DestinationToSource:
                _destinationProperty.ValueChange += DestinationProperty_ValueChange;
                _sourceProperty.Value = _destinationProperty.Value;
                break;
            case BindingType.TwoWays:
                _sourceProperty.ValueChange += SourceProperty_ValueChange;
                _destinationProperty.Value = _sourceProperty.Value;
                _destinationProperty.ValueChange += DestinationProperty_ValueChange;
                _sourceProperty.Value = _destinationProperty.Value;
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {type}");
        }
    }

    public void Unbind()
    {
        _sourceProperty.ValueChange -= SourceProperty_ValueChange;
        _destinationProperty.ValueChange -= DestinationProperty_ValueChange;
    }

    private void SourceProperty_ValueChange(TSourceParent sender, ValueChangeEventArgs<TValue> e)
    {
        _destinationProperty.Value = e.CurrentValue;
    }

    private void DestinationProperty_ValueChange(TDestinationParent sender, ValueChangeEventArgs<TValue> e)
    {
        _sourceProperty.Value = e.CurrentValue;
    }

    private void CheckValidBinding(BindableProperty<TSourceParent, TValue> sourceProperty, BindableProperty<TDestinationParent, TValue> destinationProperty, BindingType type)
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
