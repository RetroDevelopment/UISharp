using System.Reactive.Disposables;
using System.Security.Cryptography;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties.Binding;

internal class UIPropertyCollectionBinder<TSource, TDestination> : IDisposable
{
    private readonly UIPropertyCollection<TSource> _sourceProperty;
    private readonly UIPropertyCollection<TDestination> _destinationProperty;
    private readonly IBindingValueConverter<TSource, TDestination> _converter;
    private readonly BindingType _bindingType;

    private readonly CompositeDisposable _subscriptions = [];
    private bool _disposedValue;

    public UIPropertyCollectionBinder(UIPropertyCollection<TSource> sourceProperty,
                                      UIPropertyCollection<TDestination> destinationProperty,
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
                sourceProperty.IsReadOnly = false;
                destinationProperty.IsReadOnly = true;
                break;
            case BindingType.DestinationToSource:
                BindDestinationToSource();
                sourceProperty.IsReadOnly = true;
                destinationProperty.IsReadOnly = false;
                break;
            case BindingType.TwoWays:
                BindSourceToDestination();
                BindDestinationToSource();
                sourceProperty.IsReadOnly = false;
                destinationProperty.IsReadOnly = false;
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
        _destinationProperty.IsBindingTarget = true;
        SynchronizeLists(_sourceProperty, _destinationProperty, _converter.ConvertSourceToDestination);

        SubscribeAllowingEdits(
            _sourceProperty.ValueAdd,
            _sourceProperty,
            _destinationProperty,
            i => _destinationProperty.Insert(i, _converter.ConvertSourceToDestination(_sourceProperty[i])));

        SubscribeAllowingEdits(
            _sourceProperty.ValueRemove,
            _sourceProperty,
            _destinationProperty,
            _destinationProperty.RemoveAt);
    }

    private void BindDestinationToSource()
    {
        _sourceProperty.IsBindingTarget = true;
        SynchronizeLists(_destinationProperty, _sourceProperty, _converter.ConvertDestinationToSource);

        SubscribeAllowingEdits(
            _destinationProperty.ValueAdd,
            _destinationProperty,
            _sourceProperty,
            i => _sourceProperty.Insert(i, _converter.ConvertDestinationToSource(_destinationProperty[i])));

        SubscribeAllowingEdits(
            _destinationProperty.ValueRemove,
            _destinationProperty,
            _sourceProperty,
            _sourceProperty.RemoveAt);
    }

    // This method allow temporarily disabling read only constraints to allow for binding. Usually the binding target is
    // readonly to avoid inconsistencies. For example, when binding list1 -> list2, list2 is readonly to make sure that list2 is always in sync with list1,
    // since keeping a list in sync is not supported for performance reasons and to avoid complexity.
    private void SubscribeAllowingEdits<TOrigin, TTarget>(IObservable<int> observable, UIPropertyCollection<TOrigin> originProperty, UIPropertyCollection<TTarget> targetProperty, Action<int> action)
    {
        var subscription = observable.Subscribe(i =>
        {
            if (targetProperty.IsBinding) return;
            using (targetProperty.CreateBindingScope())
            using (originProperty.CreateBindingScope())
            {
                action(i);
            }
        });

        _subscriptions.Add(subscription);
    }

    private void SynchronizeLists<TOrigin, TTarget>(UIPropertyCollection<TOrigin> originProperty,
                                                    UIPropertyCollection<TTarget> targetProperty,
                                                    Func<TOrigin, TTarget> convert)
    {
        if (targetProperty.Count != 0) throw new UIPropertyValidationException($"Failed to bind {originProperty} to {targetProperty}: binding target must be empty");
        foreach (var origin in originProperty)
        {
            targetProperty.Add(convert(origin));
        }
    }

    ~UIPropertyCollectionBinder()
    {
        Dispose(disposing: false);
    }
}
