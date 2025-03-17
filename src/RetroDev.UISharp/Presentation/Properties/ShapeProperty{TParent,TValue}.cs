using System.Diagnostics;
using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Describes a property for a shape. It allows for flexible binding.
/// </summary>
/// <typeparam name="TShape">The shape owning this property.</typeparam>
/// <typeparam name="TValue">The property value type.</typeparam>
[DebuggerDisplay("{Value}")]
public class ShapeProperty<TShape, TValue> : UIProperty<TValue> where TShape : UIShape
{
    /// <summary>
    /// The <see cref="UIShape"/> owning <see langword="this" /> <see cref="ShapeProperty{TComponent, TValue}{TShape, TValue}"/>.
    /// </summary>
    public TShape Shape { get; }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The object owning this property.</param>
    /// <param name="application">The application in which this property is running.</param>
    /// <param name="value">The property value.</param>
    /// <param name="allowedBinding">
    /// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
    /// </param>
    /// <remarks>
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </remarks>
    public ShapeProperty(TShape parent, Application application, TValue value, BindingType allowedBinding = BindingType.TwoWays) : base(application, value, allowedBinding)
    {
        Shape = parent;
        ValueChange += (_, _) => Shape.Invalidate();
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The object owning this property.</param>
    /// <param name="application">The application in which this property is running.</param>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    public ShapeProperty(TShape parent, Application application, UIProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, BindingType allowedBinding = BindingType.TwoWays) : this(parent, application, destinationProperty.Value, allowedBinding)
    {
        Bind(destinationProperty, bindingType);
    }
}
