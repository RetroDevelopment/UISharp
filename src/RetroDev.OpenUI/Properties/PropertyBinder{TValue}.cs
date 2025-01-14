namespace RetroDev.OpenUI.Properties;

/// <summary>
/// A <see cref="IBinder{TValue}"/> that bind a source <see cref="BindableProperty{TParent, TValue}"/> with another, destination, <see cref="BindableProperty{TParent, TValue}"/>.
/// </summary>
/// <typeparam name="TParent">The class defining the destination property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
public class PropertyBinder<TParent, TValue> : IBinder<TValue>
{
    private readonly BindableProperty<TParent, TValue> _destinationProperty;

    /// <inheritdoc />
    public BindingType Binding { get; }

    /// <inheritdoc />
    public event EventHandler<BinderValueChangeEventArgs<TValue>> DestinationChange = (_, _) => { };

    /// <summary>
    /// Creates a new binder.
    /// </summary>
    /// <param name="destinationProperty">The destination property.</param>
    /// <param name="binding">The binding type.</param>
    public PropertyBinder(BindableProperty<TParent, TValue> destinationProperty, BindingType binding)
    {
        _destinationProperty = destinationProperty;
        Binding = binding;
        _destinationProperty.ValueChange += _destinationProperty_ValueChange;
    }

    /// <inheritdoc />
    public void NotifySourceChanged(TValue value)
    {
        _destinationProperty.Value = value;
    }

    /// <inheritdoc />
    public void Unbind() =>
        _destinationProperty.ValueChange -= _destinationProperty_ValueChange;

    private void _destinationProperty_ValueChange(TParent sender, ValueChangeEventArgs<TValue> e) =>
        DestinationChange.Invoke(sender, new BinderValueChangeEventArgs<TValue>(e.CurrentValue));
}
