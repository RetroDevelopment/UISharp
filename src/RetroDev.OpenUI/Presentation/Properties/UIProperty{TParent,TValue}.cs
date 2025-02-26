using System.Diagnostics;
using RetroDev.OpenUI.Components.Base;

namespace RetroDev.OpenUI.Presentation.Properties;

/// <summary>
/// Describes a property used in this UI framework. It allows for flexible binding.
/// </summary>
/// <typeparam name="TComponent">The class owning this property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
[DebuggerDisplay("{Value}")]
public class UIProperty<TComponent, TValue> : BindableProperty<TValue> where TComponent : UIComponent
{
    /// <summary>
    /// The <see cref="UIComponent"/> owning <see langword="this" /> <see cref="UIProperty{TComponent, TValue}"/>.
    /// </summary>
    public TComponent Component { get; }

    /// <summary>
    /// The property value.
    /// </summary>
    public override TValue Value
    {
        set
        {
            Component.Application.LifeCycle.ThrowIfPropertyCannotBeSet();
            base.Value = value;
        }
        get => base.Value;
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The object owning this property.</param>
    /// <param name="value">The property value.</param>
    /// <param name="allowedBinding">
    /// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
    /// </param>
    /// <remarks>
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </remarks>
    public UIProperty(TComponent parent, TValue value, BindingType allowedBinding = BindingType.TwoWays) : base(value, parent.Application, allowedBinding)
    {
        Component = parent;
        ValueChange += (_, _) => Component.Invalidate();
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The object owning this property.</param>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    public UIProperty(TComponent parent, BindableProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, BindingType allowedBinding = BindingType.TwoWays) : this(parent, destinationProperty.Value, allowedBinding)
    {
        Bind(destinationProperty, bindingType);
    }
}
