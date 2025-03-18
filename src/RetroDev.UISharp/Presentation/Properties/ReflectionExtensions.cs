using System.Reflection;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Provides reflection extension methods helping with properties.
/// </summary>
public static class ReflectionExtensions
{
    /// <summary>
    /// Gets the list of all UI properties defined in <paramref name="this"/> type,
    /// including <see cref="UIProperty{TValue}"/> and <see cref="UICompositeProperty{TValue}"/>.
    /// </summary>
    /// <param name="this">The type where to look the properties.</param>
    /// <returns>The list of <see cref="PropertyInfo"/> whose property type is a <see cref="UIProperty{TValue}"/>.</returns>
    public static List<PropertyInfo> GetAllUIProperties(this Type @this) =>
        @this.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.IsUIProperty() || p.IsUICompositeProperty())
             .ToList();

    /// <summary>
    /// Gets the list of <see cref="UIProperty{TValue}"/> properties defined in <paramref name="this"/> type.
    /// </summary>
    /// <param name="this">The type where to look the properties.</param>
    /// <returns>The list of <see cref="PropertyInfo"/> whose property type is assignable to <see cref="UIProperty{TValue}"/>.</returns>
    public static List<PropertyInfo> GetUIProperties(this Type @this) =>
        @this.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(IsUIProperty)
             .ToList();

    /// <summary>
    /// Check whether <paramref name="this"/> property is a <see cref="UIProperty{TValue}"/>.
    /// </summary>
    /// <param name="this">The property to check.</param>
    /// <returns>
    /// <see langword="true" /> if the return type of <paramref name="this"/> property is a subtype of
    /// <see cref="UIProperty{TValue}"/>, otherwise <see langword="false" />.
    /// </returns>
    public static bool IsUIProperty(this PropertyInfo @this) =>
        @this.PropertyType.IsUIPropertyType();

    /// <summary>
    /// Check whether <paramref name="this"/> property is a <see cref="UICompositeProperty{TValue}{TValue}"/>.
    /// </summary>
    /// <param name="this">The property to check.</param>
    /// <returns>
    /// <see langword="true" /> if the return type of <paramref name="this"/> property is a subtype of
    /// <see cref="UICompositeProperty{TValue}"/>, otherwise <see langword="false" />.
    /// </returns>
    public static bool IsUICompositeProperty(this PropertyInfo @this) =>
        @this.PropertyType.IsCompositeUIPropertyType();

    /// <summary>
    /// Check whether <paramref name="this"/> type is a <see cref="UIProperty{TValue}"/>.
    /// </summary>
    /// <param name="this">The type to check.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="this"/> type is a subtype of
    /// <see cref="UIProperty{TValue}"/>, otherwise <see langword="false" />.
    /// </returns>
    public static bool IsUIPropertyType(this Type @this) =>
        @this.IsGenericType(typeof(UIProperty<>));

    /// <summary>
    /// Check whether <paramref name="this"/> type is a <see cref="UICompositeProperty{TValue}"/>.
    /// </summary>
    /// <param name="this">The type to check.</param>
    /// <returns>
    /// <see langword="true" /> if <paramref name="this"/> type is a subtype of
    /// <see cref="UICompositeProperty{TValue}"/>, otherwise <see langword="false" />.
    /// </returns>
    public static bool IsCompositeUIPropertyType(this Type @this) =>
        @this.IsGenericType(typeof(UICompositeProperty<>));

    private static bool IsGenericType(this Type @this, Type type)
    {
        if (@this.IsGenericType && @this.GetGenericTypeDefinition() == type)
        {
            return true;
        }

        var baseType = @this.BaseType;

        if (baseType != null)
        {
            return baseType.IsGenericType(type);
        }

        return false;
    }


    /// <summary>
    /// Gets the <see cref="UIProperty{TValue}"/> value of <paramref name="this"/> <see cref="PropertyInfo"/>
    /// in the given <paramref name="instance"/>.
    /// </summary>
    /// <typeparam name="TValue">The property value type.</typeparam>
    /// <param name="this">The <see cref="PropertyInfo"/> that contains the property definition.</param>
    /// <returns><c>instance.P</c> if <c>P</c> is defined by <paramref name="this"/> <see cref="PropertyInfo"/>.</returns>
    /// <exception cref="UIPropertyValidationException">If <paramref name="this"/> <see cref="PropertyInfo"/> is not defining a property of type <see cref="UIProperty{TValue}"/>.</exception>
    public static UIProperty<TValue> GetUIProperty<TValue>(this PropertyInfo @this, object instance)
    {
        var property = @this.GetValue(instance, null);
        if (property == null) throw new UIPropertyValidationException("UI property cannot be null");
        if (property is UIProperty<TValue> uiProperty)
        {
            return uiProperty;
        }

        throw new UIPropertyValidationException($"Property info {@this} cannot be converted into a UI property of type {typeof(TValue).Name}");
    }

    /// <summary>
    /// Gets the type of <paramref name="this"/> property <see cref="UIProperty{TValue}.Value"/>.
    /// </summary>
    /// <param name="this">The property definition for which to lookup the binding type.s</param>
    /// <returns>
    /// The <see cref="UIProperty{TValue}.Value"/> type if <paramref name="this"/> <see cref="PropertyInfo"/>
    /// holds a <see cref="UIProperty{TValue}"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="this"/> <see cref="PropertyInfo"/> does not belong to a <see cref="UIProperty{TValue}"/>.
    /// </exception>
    public static Type GetBindingValueType(this PropertyInfo @this)
    {
        if (!@this.IsUIProperty()) throw new ArgumentException($"Cannot get a binding value from {@this}: this property does not describe a BindableProperty");
        var valueProperty = @this.PropertyType.GetProperty(nameof(UIProperty<object>.Value));
        if (valueProperty == null) throw new ArgumentException($"Cannot find value property");
        return valueProperty.PropertyType;
    }
}
