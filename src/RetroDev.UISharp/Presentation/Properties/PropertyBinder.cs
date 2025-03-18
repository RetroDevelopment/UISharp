using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

internal class PropertyBinder<TSource, TDestination> : IBinder, IDisposable
{
    private readonly UIProperty<TSource> _sourceProperty;
    private readonly UIProperty<TDestination> _destinationProperty;
    private readonly IBindingValueConverter<TSource, TDestination> _converter;

    private IDisposable? _sourceToDestinationSubscription;
    private IDisposable? _destinationToSourceSubscription;
    private bool _disposedValue;

    public PropertyBinder(UIProperty<TSource> sourceProperty,
                          UIProperty<TDestination> destinationProperty,
                          BindingType type,
                          IBindingValueConverter<TSource, TDestination> converter)
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
                _destinationToSourceSubscription = _destinationProperty
                    .ValueChange
                    .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
                break;
            case BindingType.TwoWays:
                _sourceToDestinationSubscription = _sourceProperty
                    .ValueChange
                    .Subscribe(v => _destinationProperty.Value = _converter.ConvertSourceToDestination(v));
                _destinationToSourceSubscription = _destinationProperty
                    .ValueChange
                    .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
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

    private void CheckValidBinding(UIProperty<TSource> sourceProperty, UIProperty<TDestination> destinationProperty, BindingType type)
    {
        switch (type)
        {
            case BindingType.SourceToDestination:
                if (!destinationProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: destination property does not allow to receive binding updates.");
                break;
            case BindingType.DestinationToSource:
                if (!sourceProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property does not allow to receive binding updates.");
                break;
            case BindingType.TwoWays:
                if (!destinationProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: destination property does not allow to receive binding updates.");
                if (!sourceProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property does not allow to receive binding updates.");
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
                _destinationToSourceSubscription?.Dispose();
            }

            _disposedValue = true;
        }
    }

    ~PropertyBinder()
    {
        Dispose(disposing: false);
    }
}
