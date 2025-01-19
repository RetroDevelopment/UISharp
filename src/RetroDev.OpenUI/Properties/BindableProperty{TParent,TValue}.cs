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
    private List<IBinder<TValue>> _binders = [];

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
                NotifySourceToDestnationBinders();
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
    /// Adds a property binder to this property.
    /// </summary>
    /// <param name="binder">The binder to bind this propery value with.</param>
    /// <exception cref="InvalidOperationException">If <paramref name="binder"/> type is either <see cref="BindingType.DestinationToSource"/>
    /// or <see cref="BindingType.TwoWays"/> and a binder with one of these two types has already been added.</exception>
    public void AddBinder(IBinder<TValue> binder)
    {
        EnsureBindingIsAllowed(binder);

        switch (binder.Binding)
        {
            case BindingType.SourceToDestination:
                binder.NotifySourceChanged(Value);
                break;
            case BindingType.DestinationToSource:
                EnsureBinderHasNoDestinationToSource();
                binder.DestinationChange += Binder_DestinationChange;
                if (binder.CurrentValue != null) Value = binder.CurrentValue;
                break;
            case BindingType.TwoWays:
                EnsureBinderHasNoDestinationToSource();
                binder.DestinationChange += Binder_DestinationChange;
                if (binder.CurrentValue != null) Value = binder.CurrentValue;
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {binder.Binding}");
        }

        _binders.Add(binder);
    }

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
        RemoveBinders();
        AddBinder(new PropertyBinder<TDestinationComponent, TValue>(property, bindingType));
    }

    /// <summary>
    /// Removes all the <see cref="IBinder{TValue}"/> bound to <see langword="this"/> property.
    /// </summary>
    public void RemoveBinders()
    {
        _binders.Where(BindsSourceToDestination)
                .ToList()
                .ForEach(binder => binder.Unbind());

        _binders.Where(BindsDestinationToSource)
                .ToList()
                .ForEach(binder => binder.DestinationChange -= Binder_DestinationChange);

        _binders.Clear();
    }


    /// <summary>
    /// Implicit cast from <see cref="BindableProperty{TParent, TValue}"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="property">The <see cref="BindableProperty{TParent, TValue}"/> to cast.</param>
    public static implicit operator TValue(BindableProperty<TParent, TValue> property) => property._value;

    private void NotifySourceToDestnationBinders()
    {
        foreach (var binder in _binders.Where(BindsSourceToDestination))
        {
            binder.NotifySourceChanged(_value);
        }
    }

    private void EnsureBinderHasNoDestinationToSource()
    {
        if (_binders.Where(BindsDestinationToSource).Count() != 0)
        {
            throw new InvalidOperationException("Only one destination to source or two ways binding allowed per property");
        }
    }

    private void EnsureBindingIsAllowed(IBinder<TValue> binder)
    {
        switch (AllowedBinding)
        {
            case BindingType.TwoWays:
                break;
            case BindingType.SourceToDestination:
                if (binder.Binding != BindingType.SourceToDestination) throw new InvalidOperationException($"Binding type not allowed {binder.Binding}: allowed binding is {AllowedBinding}");
                break;
            case BindingType.DestinationToSource:
                if (binder.Binding != BindingType.DestinationToSource) throw new InvalidOperationException($"Binding type not allowed {binder.Binding}: allowed binding is {AllowedBinding}");
                break;
        }
    }


    private bool BindsSourceToDestination(IBinder<TValue> binder) =>
        binder.Binding == BindingType.SourceToDestination || binder.Binding == BindingType.TwoWays;

    private bool BindsDestinationToSource(IBinder<TValue> binder) =>
        binder.Binding == BindingType.DestinationToSource || binder.Binding == BindingType.TwoWays;

    private void Binder_DestinationChange(object? sender, BinderValueChangeEventArgs<TValue> e)
    {
        Value = e.Value;
    }
}
