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
/// <param name="allowedBindings">The list of allowed <see cref="BindingType"/>. If <see langword="null" /> all binding types will be allowed.</param>
/// <param name="application">The application owning this property.</param>
[DebuggerDisplay("{Value}")]
public class BindableProperty<TParent, TValue>(TParent parent, TValue value, Application? application = null, List<BindingType>? allowedBindings = null)
{
    private readonly Application? _application = application;
    private readonly BindingType[] _allowedBindings = allowedBindings?.ToArray() ?? Enum.GetValues<BindingType>();
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
    /// Adds a property binder to this property.
    /// </summary>
    /// <param name="binder">The binder to bind this propery value with.</param>
    /// <exception cref="InvalidOperationException">If <paramref name="binder"/> type is either <see cref="BindingType.DestinationToSource"/>
    /// or <see cref="BindingType.TwoWays"/> and a binder with one of these two types has already been added.</exception>
    public void AddBinder(IBinder<TValue> binder)
    {
        if (_allowedBindings != null && !_allowedBindings.Contains(binder.Binding)) throw new InvalidOperationException($"Binding {binder.Binding} not allowed");

        switch (binder.Binding)
        {
            case BindingType.SourceToDestination:
                break;
            case BindingType.DestinationToSource:
                EnsureBinderHasNoDestinationToSource();
                binder.DestinationChange += Binder_DestinationChange;
                break;
            case BindingType.TwoWays:
                EnsureBinderHasNoDestinationToSource();
                binder.DestinationChange += Binder_DestinationChange;
                break;
            default:
                throw new ArgumentException($"Unhandled binding type {binder.Binding}");
        }

        _binders.Add(binder);
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

    private bool BindsSourceToDestination(IBinder<TValue> binder) =>
        binder.Binding == BindingType.SourceToDestination || binder.Binding == BindingType.TwoWays;

    private bool BindsDestinationToSource(IBinder<TValue> binder) =>
        binder.Binding == BindingType.DestinationToSource || binder.Binding == BindingType.TwoWays;

    private void Binder_DestinationChange(object? sender, BinderValueChangeEventArgs<TValue> e)
    {
        Value = e.Value;
    }
}
