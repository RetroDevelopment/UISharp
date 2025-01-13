namespace RetroDev.OpenUI.Properties;

// TODO: other than property binders we can have other types of binders?
// e.g. DB binder where NotifySourceChanged() is storing to database

/// <summary>
/// Defines a generic binder that can update a <see cref="BindableProperty{TParent, TValue}"/> value and conversely
/// be notifying when such value change.
/// A <see cref="BindableProperty{TParent, TValue}"/> is called <strong>source</strong> property and this <see cref="IBinder{TValue}"/> is
/// representing some sort of value called <strong>destination</strong> value, which could be another <see cref="BindableProperty{TParent, TValue}"/>,
/// a file, a database table field, etc.
/// The idea is to automatically keep in sync source with destination. If the binding is <see cref="BindingType.SourceToDestination"/>, changes
/// to the source property must be reflected to the destination property (e.g. changes to a UI text will be reflected to the corrsponding database column, or to the
/// model view respective property). If the binding is <see cref="BindingType.DestinationToSource"/> the opposite happens: changes to the destination are reflected to the source
/// (e.g. a file change is reflected to a UI property, or a view model change is reflected to the UI property).
/// If the binding is <see cref="BindingType.TwoWays"/> both source to destination and destination to source bindings apply.
/// </summary>
/// <typeparam name="TValue">The property value type.</typeparam>
public interface IBinder<TValue>
{
    /// <summary>
    /// Specifies how to bind.
    /// </summary>
    /// <seealso cref="BindingType"/>
    BindingType Binding { get; }

    /// <summary>
    /// Triggered when it is time to notify the source binder that the destination value has changed.
    /// </summary>
    event EventHandler<BinderValueChangeEventArgs<TValue>> DestinationChange;

    /// <summary>
    /// Called when the source property value changes.
    /// </summary>
    /// <param name="value">The updated value.</param>
    void NotifySourceChanged(TValue value);

    /// <summary>
    /// Stops notifying the source property of this binder changes.
    /// </summary>
    void Unbind();
}

