using System.Diagnostics;
using RetroDev.OpenUI.Events;

namespace RetroDev.OpenUI.Properties;

/// <summary>
/// A property wrapper that allows for dynamic one or two way binding.
/// </summary>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <param name="value">The property value.</param>
/// <param name="allowedBinding">
/// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
/// </param>
/// <param name="application">The application owning this property.</param>
/// <remarks>
/// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
/// </remarks>
[DebuggerDisplay("{Value}")]
public class BindableProperty<TValue>(TValue value, Application? application = null, BindingType allowedBinding = BindingType.TwoWays)
{
    private readonly Application? _application = application;
    private TValue _value = value;
    private IBinder? _binder;

    /// <summary>
    /// Triggers then the <see cref="Value"/> changes. Setting <see cref="Value"/> to the same value
    /// does not trigger this event, only modifying the value does.
    /// </summary>
    public event TypeSafeEventHandler<BindableProperty<TValue>, ValueChangeEventArgs<TValue>> ValueChange = (_, _) => { };

    /// <summary>
    /// The property value.
    /// </summary>
    public virtual TValue Value
    {
        set
        {
            _application?.LifeCycle?.ThrowIfPropertyCannotBeSet();

            if (!EqualityComparer<TValue>.Default.Equals(_value, value))
            {
                var previousValue = _value;
                _value = value;
                ValueChange.Invoke(this, new ValueChangeEventArgs<TValue>(previousValue, value));
            }
        }
        get
        {
            _application?.LifeCycle?.ThrowIfNotOnUIThread();
            return _value;
        }
    }


    /// <summary>
    /// The allowed <see cref="BindingType"/>.
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed,
    /// including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </summary>
    public BindingType AllowedBinding { get; } = allowedBinding;

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="application">The application owning this property.</param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    public BindableProperty(BindableProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, Application? application = null, BindingType allowedBinding = BindingType.TwoWays) : this(destinationProperty.Value, application, allowedBinding)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void Bind(BindableProperty<TValue> destinationProperty, BindingType bindingType)
    {
        _binder?.Unbind();
        _binder = new PropertyBinder<TValue>(this, destinationProperty, bindingType);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindSourceToDestination(BindableProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.SourceToDestination);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindDestinationToSource(BindableProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.DestinationToSource);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindSourceTwoWays(BindableProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.TwoWays);
    }

    /// <summary>
    /// Removes a binding if any.
    /// </summary>
    public void RemoveBinding()
    {
        _binder?.Unbind();
    }

    /// <summary>
    /// Implicit cast from <see cref="BindableProperty{TValue}"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="property">The <see cref="BindableProperty{TValue}"/> to cast.</param>
    public static implicit operator TValue(BindableProperty<TValue> property) => property._value;
}
