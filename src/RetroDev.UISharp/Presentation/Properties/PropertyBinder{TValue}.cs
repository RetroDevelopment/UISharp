using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

internal class PropertyBinder<TSourceValue, TDestinationValue> : IBinder, IDisposable
{
    private readonly UIProperty<TSourceValue> _sourceProperty;
    private readonly UIProperty<TDestinationValue> _destinationProperty;
    private readonly IBindingValueConverter<TSourceValue, TDestinationValue> _converter;

    private IDisposable? _sourceToDestinationSubscription;
    private IDisposable? _destinationToDestinationSubscription;
    private bool _disposedValue;

    public PropertyBinder(UIProperty<TSourceValue> sourceProperty,
                          UIProperty<TDestinationValue> destinationProperty,
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
                _sourceToDestinationSubscription = _sourceProperty
                    .ValueChange
                    .Subscribe(v => _destinationProperty.Value = _converter.ConvertSourceToDestination(v));
                break;
            case BindingType.DestinationToSource:
                _destinationToDestinationSubscription = _destinationProperty
                    .ValueChange
                    .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
                break;
            case BindingType.TwoWays:
                _destinationToDestinationSubscription = _destinationProperty
                    .ValueChange
                    .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
                _sourceToDestinationSubscription = _sourceProperty
                    .ValueChange
                    .Subscribe(v => _destinationProperty.Value = _converter.ConvertSourceToDestination(v));
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {type}");
        }
    }

    public void Unbind() => Dispose();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void CheckValidBinding(UIProperty<TSourceValue> sourceProperty, UIProperty<TDestinationValue> destinationProperty, BindingType type)
    {
        switch (type)
        {
            case BindingType.SourceToDestination:
                if (sourceProperty.AllowedBinding != BindingType.SourceToDestination && sourceProperty.AllowedBinding != BindingType.TwoWays) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: source property does not allow for that binding.");
                if (destinationProperty.AllowedBinding != BindingType.DestinationToSource && destinationProperty.AllowedBinding != BindingType.TwoWays) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: destination property does not allow for that binding.");
                break;
            case BindingType.DestinationToSource:
                if (sourceProperty.AllowedBinding != BindingType.DestinationToSource && sourceProperty.AllowedBinding != BindingType.TwoWays) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property does not allow for that binding.");
                if (destinationProperty.AllowedBinding != BindingType.SourceToDestination && destinationProperty.AllowedBinding != BindingType.TwoWays) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: destination property does not allow for that binding.");
                break;
            case BindingType.TwoWays:
                if (sourceProperty.AllowedBinding != BindingType.TwoWays) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <-> {destinationProperty}: source property does not allow for two way binding.");
                if (destinationProperty.AllowedBinding != BindingType.TwoWays) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <-> {destinationProperty}: destination property does not allow for two way binding.");
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {type}");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _sourceToDestinationSubscription?.Dispose();
                _destinationToDestinationSubscription?.Dispose();
            }

            _disposedValue = true;
        }
    }

    ~PropertyBinder()
    {
        Dispose(disposing: false);
    }
}
