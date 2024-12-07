namespace RetroDev.OpenUI.Properties;

/// <summary>
/// A <see cref="IBinder{TValue}"/> that bind a source property with another, destination, property.
/// </summary>
/// <typeparam name="TParent">The class defining the destination propertys.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
public class PropertyBinder<TParent, TValue> : IBinder<TValue>
{
    private readonly UIProperty<TParent, TValue> _destinationProperty;

    /// <inheritdoc />
    public BindingType Binding { get; }

    /// <summary>
    /// Creates a new binder.
    /// </summary>
    /// <param name="destinationProperty">The destination property.</param>
    /// <param name="binding">The binding type.</param>
    public PropertyBinder(UIProperty<TParent, TValue> destinationProperty, BindingType binding)
    {
        _destinationProperty = destinationProperty;
        Binding = binding;
        _destinationProperty.ValueChange += (sender, e) => { DestinationChange.Invoke(sender, new BinderValueChangeEventArgs<TValue>(e.CurrentValue)); };
    }

    /// <inheritdoc />
    public event EventHandler<BinderValueChangeEventArgs<TValue>> DestinationChange = (_, _) => { };

    /// <inheritdoc />
    public void NotifySourceChanged(TValue value)
    {
        _destinationProperty.Value = value;
    }
}
