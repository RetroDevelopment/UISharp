namespace RetroDev.UISharp.Presentation.Properties.Binding;

/// <summary>
/// A value converter factory class.
/// </summary>
public static class ValueConverterFactory
{
    /// <summary>
    /// Creates a value converter from lambdas.
    /// </summary>
    /// <typeparam name="TSource">The source property value type.</typeparam>
    /// <typeparam name="TDestination">The destination property value type.</typeparam>
    /// <param name="sourceToDestination">The lambda converting from source values to destination values. If omitted, a lambda that throws <see cref="NotSupportedException"/> will be created.</param>
    /// <param name="destinationToSource">The lambda converting from destination values to source values. If omitted, a lambda that throws <see cref="NotSupportedException"/> will be created.</param>
    /// <returns>The <see cref="IBindingValueConverter{TSourceValue, TDestinationValue}"/> defined by the given lambdas.</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static IBindingValueConverter<TSource, TDestination> FromLambda<TSource, TDestination>(Func<TSource, TDestination>? sourceToDestination = null,
                                                                                                  Func<TDestination, TSource>? destinationToSource = null) =>
        new LambdaValueConverter<TSource, TDestination>(sourceToDestination ?? (_ => throw new NotSupportedException()),
                                                        destinationToSource ?? (_ => throw new NotSupportedException()));

    /// <summary>
    /// Creates a value converter with identity functions. This is used when source and destination values have the same type.
    /// </summary>
    /// <typeparam name="TValue">The source and destination properties value type.</typeparam>
    /// <returns>The <see cref="IBindingValueConverter{TSourceValue, TDestinationValue}"/> represented by the identity function.</returns>
    public static IBindingValueConverter<TValue, TValue> Identity<TValue>() =>
        new LambdaValueConverter<TValue, TValue>(v => v, v => v);

    private class LambdaValueConverter<TSource, TDestination>(Func<TSource, TDestination> sourceToDestination,
                                                              Func<TDestination, TSource> destinationToSource) : IBindingValueConverter<TSource, TDestination>
    {
        public TDestination ConvertSourceToDestination(TSource value) =>
            sourceToDestination(value);

        public TSource ConvertDestinationToSource(TDestination value) =>
            destinationToSource(value);
    }
}
