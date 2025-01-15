using System.Diagnostics;
using RetroDev.OpenUI.Components;

namespace RetroDev.OpenUI.Properties;

/// <summary>
/// Describes a property used in this UI framework. It allows for flexible binding.
/// </summary>
/// <typeparam name="TParent">The class owning this property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <param name="parent">The object owning this property.</param>
/// <param name="value">The property value.</param>
/// <param name="allowedBindings">The list of allowed <see cref="BindingType"/>. If <see langword="null" /> all binding types will be allowed.</param>
[DebuggerDisplay("{Value}")]
public class UIProperty<TParent, TValue>(TParent parent, TValue value, List<BindingType>? allowedBindings = null) : BindableProperty<TParent, TValue>(parent, value, parent.Application, allowedBindings) where TParent : UIComponent
{
    /// <summary>
    /// The property value.
    /// </summary>
    public override TValue Value
    {
        set
        {
            base.Value = value;
            Parent.Application._eventSystem.InvalidateRendering(); // TODO: do not push one event for each call but just one if the rendering has not been invalidated yet
        }
        get => base.Value;
    }

    /// <summary>
    /// Implicit cast from <see cref="UIProperty{TParent, TValue}"/> to <typeparamref name="TValue"/>.
    /// </summary>
    /// <param name="property">The <see cref="UIProperty{TParent, TValue}"/> to cast.</param>
    public static implicit operator TValue(UIProperty<TParent, TValue> property) => property.Value;

    private void EnsureCanSetUIComponentProperty(UIComponent uiParent)
    {
        uiParent.Application.LifeCycle.ThrowIfPropertyCannotBeSet();
    }
}
