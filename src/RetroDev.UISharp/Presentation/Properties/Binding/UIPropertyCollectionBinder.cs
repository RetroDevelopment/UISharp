using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties.Binding;

internal class UIPropertyCollectionBinder<TSource, TDestination> : IDisposable
{
    private readonly UIPropertyCollection<TSource> _sourceProperty;
    private readonly UIPropertyCollection<TDestination> _destinationProperty;
    private readonly BindingType _bindingType;

    private readonly List<IDisposable> _subscriptions = [];
    private bool _disposedValue;

    public UIPropertyCollectionBinder(UIPropertyCollection<TSource> sourceProperty,
                                      UIPropertyCollection<TDestination> destinationProperty,
                                      BindingType bindingType,
                                      IBindingValueConverter<TSource, TDestination> converter)
    {
        _sourceProperty = sourceProperty;
        _destinationProperty = destinationProperty;
        _bindingType = bindingType;

        CheckValidBinding();

        switch (bindingType)
        {
            case BindingType.SourceToDestination:
                BindSourceToDestination(converter);
                sourceProperty.IsReadOnly = false;
                destinationProperty.IsReadOnly = true;
                break;
            case BindingType.DestinationToSource:
                BindDestinationToSource(converter);
                sourceProperty.IsReadOnly = true;
                destinationProperty.IsReadOnly = false;
                break;
            case BindingType.TwoWays:
                BindSourceToDestination(converter);
                BindDestinationToSource(converter);
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

                _subscriptions.ForEach(subscription => subscription.Dispose());
                _subscriptions.Clear();
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

    private void BindSourceToDestination(IBindingValueConverter<TSource, TDestination> converter)
    {
        _destinationProperty.IsBindingTarget = true;

        SubscribeAllowingEdits(
            _sourceProperty.ValueAdd,
            _destinationProperty,
            i => _destinationProperty.Insert(i, converter.ConvertSourceToDestination(_sourceProperty[i])));

        SubscribeAllowingEdits(
            _sourceProperty.ValueRemove,
            _destinationProperty,
            _destinationProperty.RemoveAt);

        SubscribeAllowingEdits(
            _sourceProperty.ValueChange,
            _destinationProperty,
            i => _destinationProperty[i] = converter.ConvertSourceToDestination(_sourceProperty[i]));
    }

    private void BindDestinationToSource(IBindingValueConverter<TSource, TDestination> converter)
    {
        _sourceProperty.IsBindingTarget = true;

        SubscribeAllowingEdits(
            _destinationProperty.ValueAdd,
            _sourceProperty,
            i => _sourceProperty.Insert(i, converter.ConvertDestinationToSource(_destinationProperty[i])));

        SubscribeAllowingEdits(
            _destinationProperty.ValueRemove,
            _sourceProperty,
            _sourceProperty.RemoveAt);

        SubscribeAllowingEdits(
            _destinationProperty.ValueChange,
            _sourceProperty,
            i => _sourceProperty[i] = converter.ConvertDestinationToSource(_destinationProperty[i]));
    }

    // This method allow temporarily disabling read only constraints to allow for binding. Usually the binding target is
    // readonly to avoid inconsistencies. For example, when binding list1 -> list2, list2 is readonly to make sure that list2 is always in sync with list1,
    // since keeping a list in sync is not supported for performance reasons and to avoid complexity.
    private void SubscribeAllowingEdits<TValue>(IObservable<int> observable, UIPropertyCollection<TValue> targetProperty, Action<int> action)
    {
        var subscription = observable.Subscribe(i =>
        {
            var isReadOnly = targetProperty.IsReadOnly;
            targetProperty.IsReadOnly = false;
            action(i);
            targetProperty.IsReadOnly = isReadOnly;
        });

        _subscriptions.Add(subscription);
    }

    ~UIPropertyCollectionBinder()
    {
        Dispose(disposing: false);
    }
}
