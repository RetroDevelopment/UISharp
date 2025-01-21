using System.Diagnostics;
using RetroDev.OpenUI.Events;

namespace RetroDev.OpenUI.Properties;

/// <summary>
/// A property wrapper that allows for dynamic one or two way binding.
/// </summary>
/// <typeparam name="TParent">The class owning this property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <param name="parent">The object owning this property.</param>
/// <param name="value">The property value.</param>
/// <param name="allowedBinding">
/// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
/// </param>
/// <param name="application">The application owning this property.</param>
/// <remarks>
/// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
/// </remarks>
[DebuggerDisplay("{Value}")]
public class BindableProperty<TParent, TValue>(TParent parent, TValue value, Application? application = null, BindingType allowedBinding = BindingType.TwoWays)
{
    private readonly Application? _application = application;
    private TValue _value = value;
    private IBinder? _binder;

    /// <summary>
    /// Triggers then the <see cref="Value"/> changes. Setting <see cref="Value"/> to the same value
    /// does not trigger this event, only modifying the value does.
    /// </summary>
    public event TypeSafeEventHandler<TParent, ValueChangeEventArgs<TValue>> ValueChange = (_, _) => { };

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
                ValueChange.Invoke(Parent, new ValueChangeEventArgs<TValue>(previousValue, value));
            }
        }
        get
        {
            _application?.LifeCycle?.ThrowIfNotOnUIThread();
            return _value;
        }
    }

    /// <summary>
    /// The object owning this property.
    /// </summary>
    public TParent Parent => parent;

    /// <summary>
    /// The allowed <see cref="BindingType"/>.
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed,
    /// including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </summary>
    public BindingType AllowedBinding { get; } = allowedBinding;

    /// <summary>
    /// Binds <see langword="this" /> property to the given property and removes every existing binding.
    /// </summary>
    /// <typeparam name="TDestinationComponent">The <see cref="UIComponent"/> owrning the given <paramref name="property"/> to bind.</typeparam>
    /// <param name="property">The destination property to bind.</param>
    /// <param name="bindingType"The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="property" /> is the destination property.
    /// </param>
    public void Bind<TDestinationComponent>(BindableProperty<TDestinationComponent, TValue> property, BindingType bindingType)
    {
        _binder?.Unbind();
        _binder = new PropertyBinder<TValue, TParent, TDestinationComponent>(this, property, bindingType);
    }

    /// <summary>
    /// Removes a binding if any.
    /// </summary>
    public void RemoveBinding()
    {
        _binder?.Unbind();
    }

    /// <summary>
    /// Implicit cast from <see cref="BindableProperty{TParent, TValue}"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="property">The <see cref="BindableProperty{TParent, TValue}"/> to cast.</param>
    public static implicit operator TValue(BindableProperty<TParent, TValue> property) => property._value;
}
