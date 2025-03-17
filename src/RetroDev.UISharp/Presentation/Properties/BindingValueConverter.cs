namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// This class converts values of type <typeparamref name="TSourceValue"/> into values of type <typeparamref name="TDestinationValue"/>
/// and vice-versa. It is used to bind values of different types.
/// </summary>
/// <typeparam name="TSourceValue">Any type representing the source property value type.</typeparam>
/// <typeparam name="TDestinationValue">Any type representing the destination property value type.</typeparam>
public interface IBindingValueConverter<TSourceValue, TDestinationValue>
{
    /// <summary>
    /// Converts <paramref name="value"/> into a value of type <typeparamref name="TSourceValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="TSourceValue"/>.</returns>
    TSourceValue ConvertDestinationToSource(TDestinationValue value);

    /// <summary>
    /// Converts <paramref name="value"/> into a value of type <typeparamref name="TDestinationValue"/>.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="TDestinationValue"/>.</returns>
    TDestinationValue ConvertSourceToDestination(TSourceValue value);
}
