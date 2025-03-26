namespace RetroDev.UISharp.Presentation.Properties.Binding;

/// <summary>
/// This class converts values of type <typeparamref name="TSource"/> into values of type <typeparamref name="TDestination"/>
/// and vice-versa. It is used to bind values of different types.
/// </summary>
/// <typeparam name="TSource">Any type representing the source property value type.</typeparam>
/// <typeparam name="TDestination">Any type representing the destination property value type.</typeparam>
public interface IBindingValueConverter<TSource, TDestination>
{
    /// <summary>
    /// Converts <paramref name="source"/> into a value of type <typeparamref name="TDestination"/>.
    /// </summary>
    /// <param name="source">The value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="TDestination"/>.</returns>
    TDestination ConvertSourceToDestination(TSource source);

    /// <summary>
    /// Converts <paramref name="destination"/> into a value of type <typeparamref name="TSource"/>.
    /// </summary>
    /// <param name="destination">The value to convert.</param>
    /// <returns>The converted value of type <typeparamref name="TSource"/>.</returns>
    TSource ConvertDestinationToSource(TDestination destination);
}
