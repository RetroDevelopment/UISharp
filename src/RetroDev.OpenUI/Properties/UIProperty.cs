using System.Diagnostics;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Events;

namespace RetroDev.OpenUI.Properties;

/// <summary>
/// Describes a property used in this UI framework. It allows for flexible binding.
/// </summary>
/// <typeparam name="TParent">The class owning this property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <param name="parent">The object owning this property.</param>
/// <param name="value">The property value.</param>
[DebuggerDisplay("{Value}")]
public class UIProperty<TParent, TValue>(TParent parent, TValue value)
{
    private readonly TParent _parent = parent;
    private TValue _value = value;
    private List<IBinder<TValue>> _binders = [];

    /// <summary>
    /// Triggers then the <see cref="Value"/> changes. Setting <see cref="Value"/> does not trigger
    /// this event unless its value has actually changed.
    /// </summary>
    public event TypeSafeEventHandler<TParent, ValueChangeEventArgs<TValue>> ValueChange = (_, _) => { };

    /// <summary>
    /// The property value.
    /// </summary>
    public TValue Value
    {
        set
        {
            if (_parent is UIComponent uiParent)
            {
                EnsureCanSetUIComponentProperty(uiParent);
                uiParent.Application._eventSystem.InvalidateRendering(); // TODO: do not push one event for each call but just one if the rendering has not been invalidated yet
            }

            if (_value != null && !_value.Equals(value) || _value == null && value != null) // TODO: maybe implement equatable?
            {
                var previousValue = _value;
                _value = value;
                ValueChange.Invoke(_parent, new ValueChangeEventArgs<TValue>(previousValue, value));
                NotifySourceToDestnationBinders();
            }
        }
        get
        {
            if (_parent is UIComponent uiParent)
            {
                uiParent.Application.LifeCycle.ThrowIfNotOnUIThread();
            }

            return _value;
        }
    }

    /// <summary>
    /// Adds a property binder to this property.
    /// </summary>
    /// <param name="binder">The binder to bind this propery value with.</param>
    /// <exception cref="InvalidOperationException">If <paramref name="binder"/> type is either <see cref="BindingType.DestinationToSource"/>
    /// or <see cref="BindingType.TwoWays"/> and a binder with one of these two types has already been added.</exception>
    public void AddBinder(IBinder<TValue> binder)
    {
        switch (binder.Binding)
        {
            case BindingType.SourceToDestination:
                _binders.Add(binder);
                break;
            case BindingType.DestinationToSource:
                EnsureBinderHasNoDestinationToSource();
                binder.DestinationChange += (_, e) => { Value = e.Value; };
                break;
            case BindingType.TwoWays:
                EnsureBinderHasNoDestinationToSource();
                binder.DestinationChange += (_, e) => { Value = e.Value; };
                break;
        }
    }

    /// <summary>
    /// Removes all the <see cref="IBinder{TValue}"/> bound to <see langword="this"/> property.
    /// </summary>
    public void RemoveBinders()
    {
        _binders.Clear();
    }

    public static implicit operator TValue(UIProperty<TParent, TValue> property) => property._value;

    private void EnsureCanSetUIComponentProperty(UIComponent uiParent)
    {
        uiParent.Application.LifeCycle.ThrowIfPropertyCannotBeSet();
    }

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
}
