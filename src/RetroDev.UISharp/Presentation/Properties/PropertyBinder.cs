using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

internal class PropertyBinder<TSource, TDestination> : IBinder, IDisposable
{
    private readonly UIProperty<TSource> _sourceProperty;
    private readonly UIProperty<TDestination> _destinationProperty;
    private readonly BindingType _bindingType;
    private readonly IBindingValueConverter<TSource, TDestination> _converter;

    private IDisposable? _sourceToDestinationSubscription;
    private IDisposable? _destinationToSourceSubscription;
    private bool _disposedValue;

    public PropertyBinder(UIProperty<TSource> sourceProperty,
                          UIProperty<TDestination> destinationProperty,
                          BindingType bindingType,
                          IBindingValueConverter<TSource, TDestination> converter)
    {
        CheckValidBinding(sourceProperty, destinationProperty, bindingType);

        _sourceProperty = sourceProperty;
        _destinationProperty = destinationProperty;
        _bindingType = bindingType;
        _converter = converter;

        switch (bindingType)
        {
            case BindingType.SourceToDestination:
                _sourceToDestinationSubscription = _sourceProperty
                    .ValueChange
                    .Subscribe(v => _destinationProperty.Value = _converter.ConvertSourceToDestination(v));
                _destinationProperty.IsBindingTarget = true;
                break;
            case BindingType.DestinationToSource:
                _destinationToSourceSubscription = _destinationProperty
                    .ValueChange
                    .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
                _sourceProperty.IsBindingTarget = true;
                break;
            case BindingType.TwoWays:
                _sourceToDestinationSubscription = _sourceProperty
                    .ValueChange
                    .Subscribe(v => _destinationProperty.Value = _converter.ConvertSourceToDestination(v));
                _destinationToSourceSubscription = _destinationProperty
                    .ValueChange
                    .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
                _sourceProperty.IsBindingTarget = true;
                _destinationProperty.IsBindingTarget = true;
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {bindingType}");
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
                if (destinationProperty.IsBindingTarget) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: destination property is already a target of another binding");
                break;
            case BindingType.DestinationToSource:
                if (!sourceProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property does not allow to receive binding updates.");
                if (sourceProperty.IsBindingTarget) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property is already a target of another binding");
                break;
            case BindingType.TwoWays:
                if (!destinationProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: destination property does not allow to receive binding updates.");
                if (!sourceProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property does not allow to receive binding updates.");
                if (destinationProperty.IsBindingTarget) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} -> {destinationProperty}: destination property is already a target of another binding");
                if (sourceProperty.IsBindingTarget) throw new UIPropertyValidationException($"Invalid binding {sourceProperty} <- {destinationProperty}: source property is already a target of another binding");
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
                switch (_bindingType)
                {
                    case BindingType.SourceToDestination:
                        _destinationProperty.IsBindingTarget = false;
                        break;
                    case BindingType.DestinationToSource:
                        _sourceProperty.IsBindingTarget = false;
                        break;
                    case BindingType.TwoWays:
                        _sourceProperty.IsBindingTarget = false;
                        _destinationProperty.IsBindingTarget = false;
                        break;
                    default:
                        throw new ArgumentException($"Unhandled binding type {_bindingType}");
                }
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
