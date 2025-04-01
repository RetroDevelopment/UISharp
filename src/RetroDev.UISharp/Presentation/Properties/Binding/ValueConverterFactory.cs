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
        new LambdaValueConverter<TSource, TDestination>(sourceToDestination, destinationToSource);

    /// <summary>
    /// Creates a value converter with identity functions. This is used when source and destination values have the same type.
    /// </summary>
    /// <typeparam name="TValue">The source and destination properties value type.</typeparam>
    /// <returns>The <see cref="IBindingValueConverter{TSourceValue, TDestinationValue}"/> represented by the identity function.</returns>
    public static IBindingValueConverter<TValue, TValue> Identity<TValue>() =>
        new LambdaValueConverter<TValue, TValue>(value => value, value => value);

    /// <summary>
    /// Creates a value converter with identity functions. This is used when source and destination values have the same type.
    /// </summary>
    /// <typeparam name="TValue">The source and destination properties value type.</typeparam>
    /// <returns>The <see cref="IBindingValueConverter{TSourceValue, TDestinationValue}"/> represented by the identity function.</returns>
    public static IHierarchicalBindingValueConverter<TValue, TValue> HierarchicalIdentity<TValue>() =>
        new LambdaValueConverter<TValue, TValue>(value => value, value => value, node => node, node => node);

    /// <summary>
    /// Flips <paramref name="this"/> converter source and destination types.
    /// </summary>
    /// <typeparam name="TSource"><paramref name="this"/> converter source value type.</typeparam>
    /// <typeparam name="TDestination"><paramref name="this"/> converter destination value type.</typeparam>
    /// <param name="this">The converter.</param>
    /// <returns>A converter that converts to and from the opposite values as <paramref name="this"/>.</returns>
    public static IBindingValueConverter<TDestination, TSource> Flip<TSource, TDestination>(this IBindingValueConverter<TSource, TDestination> @this) =>
        new LambdaValueConverter<TDestination, TSource>(@this.ConvertDestinationToSource, @this.ConvertSourceToDestination);

    /// <summary>
    /// Flips <paramref name="this"/> converter source and destination types.
    /// </summary>
    /// <typeparam name="TSource"><paramref name="this"/> converter source value type.</typeparam>
    /// <typeparam name="TDestination"><paramref name="this"/> converter destination value type.</typeparam>
    /// <param name="this">The converter.</param>
    /// <returns>A converter that converts to and from the opposite values as <paramref name="this"/>.</returns>
    public static IHierarchicalBindingValueConverter<TDestination, TSource> Flip<TSource, TDestination>(this IHierarchicalBindingValueConverter<TSource, TDestination> @this) =>
        new LambdaValueConverter<TDestination, TSource>(@this.ConvertDestinationToSource, @this.ConvertSourceToDestination, @this.ConvertDestinationToSource, @this.ConvertSourceToDestination);

    private class LambdaValueConverter<TSource, TDestination>(Func<TSource, TDestination>? sourceToDestination = null,
                                                              Func<TDestination, TSource>? destinationToSource = null,
                                                              Func<UITreeNode<TSource>, UITreeNode<TDestination>>? hSourceToDestination = null,
                                                              Func<UITreeNode<TDestination>, UITreeNode<TSource>>? hDestinationToSource = null) : IHierarchicalBindingValueConverter<TSource, TDestination>
    {
        public TDestination ConvertSourceToDestination(TSource source) =>
            InvokeOrThrow(sourceToDestination, source);

        public TSource ConvertDestinationToSource(TDestination destination) =>
            InvokeOrThrow(destinationToSource, destination);

        public UITreeNode<TDestination> ConvertSourceToDestination(UITreeNode<TSource> source) =>
            InvokeOrThrow(hSourceToDestination, source);

        public UITreeNode<TSource> ConvertDestinationToSource(UITreeNode<TDestination> destination) =>
            InvokeOrThrow(hDestinationToSource, destination);

        private TOutput InvokeOrThrow<TInput, TOutput>(Func<TInput, TOutput>? function, TInput input) =>
            function is not null ? function(input) : throw new NotSupportedException();
    }
}
