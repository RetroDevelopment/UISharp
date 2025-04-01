using System.Reactive.Disposables;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties.Binding;

internal class UIPropertyBinder<TSource, TDestination> : IDisposable
{
    private readonly UIProperty<TSource> _sourceProperty;
    private readonly UIProperty<TDestination> _destinationProperty;
    private readonly IBindingValueConverter<TSource, TDestination> _converter;
    private readonly BindingType _bindingType;

    private readonly CompositeDisposable _subscriptions = [];
    private bool _disposedValue;

    public UIPropertyBinder(UIProperty<TSource> sourceProperty,
                            UIProperty<TDestination> destinationProperty,
                            BindingType bindingType,
                            IBindingValueConverter<TSource, TDestination> converter)
    {
        _sourceProperty = sourceProperty;
        _destinationProperty = destinationProperty;
        _converter = converter;
        _bindingType = bindingType;

        CheckValidBinding();

        switch (bindingType)
        {
            case BindingType.SourceToDestination:
                BindSourceToDestination();
                break;
            case BindingType.DestinationToSource:
                BindDestinationToSource();
                break;
            case BindingType.TwoWays:
                BindSourceToDestination();
                BindDestinationToSource();
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {bindingType}");
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
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

                _subscriptions.Dispose();
            }

            _disposedValue = true;
        }
    }
    private void CheckValidBinding()
    {
        switch (_bindingType)
        {
            case BindingType.SourceToDestination:
                ValidateSourceToDestinationBinding();
                break;
            case BindingType.DestinationToSource:
                ValidateDestinationToSourceBinding();
                break;
            case BindingType.TwoWays:
                ValidateSourceToDestinationBinding();
                ValidateDestinationToSourceBinding();
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {_bindingType}");
        }
    }

    private void ValidateSourceToDestinationBinding()
    {
        if (!_destinationProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {_sourceProperty} -> {_destinationProperty}: destination property does not allow to receive binding updates.");
        if (_destinationProperty.IsBindingTarget) throw new UIPropertyValidationException($"Invalid binding {_sourceProperty} -> {_destinationProperty}: destination property is already a target of another binding");
    }

    private void ValidateDestinationToSourceBinding()
    {
        if (!_sourceProperty.CanReceiveBindingUpdates) throw new UIPropertyValidationException($"Invalid binding {_sourceProperty} <- {_destinationProperty}: source property does not allow to receive binding updates.");
        if (_sourceProperty.IsBindingTarget) throw new UIPropertyValidationException($"Invalid binding {_sourceProperty} <- {_destinationProperty}: source property is already a target of another binding");
    }

    private void BindSourceToDestination()
    {
        var subscription = _sourceProperty
            .ValueChange
            .Subscribe(v => _destinationProperty.Value = _converter.ConvertSourceToDestination(v));
        _subscriptions.Add(subscription);
        _destinationProperty.IsBindingTarget = true;
    }

    private void BindDestinationToSource()
    {
        var subscription = _destinationProperty
            .ValueChange
            .Subscribe(v => _sourceProperty.Value = _converter.ConvertDestinationToSource(v));
        _subscriptions.Add(subscription);
        _sourceProperty.IsBindingTarget = true;
    }

    ~UIPropertyBinder()
    {
        Dispose(disposing: false);
    }
}
