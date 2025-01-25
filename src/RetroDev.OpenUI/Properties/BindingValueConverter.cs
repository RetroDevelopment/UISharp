namespace RetroDev.OpenUI.Properties;

/// <summary>
/// This class converts values of type <typeparamref name="TValue1"/> into values of type <typeparamref name="TValue2"/>
/// and viceversa. It is used to bind values of different types.
/// </summary>
/// <typeparam name="TValue1">Any type.</typeparam>
/// <typeparam name="TValue2">Any type.</typeparam>
public interface IBindingValueConverter<TValue1, TValue2>
{
    /// <summary>
    /// Converts <paramref name="value"/> into a value of type <typeparamref name="TValue1"/>.
    /// </summary>
    /// <param name="value">The value to conver.</param>
    /// <returns>The converted value of type <typeparamref name="TValue1"/>.</returns>
    TValue1 Convert(TValue2 value);

    /// <summary>
    /// Converts <paramref name="value"/> into a value of type <typeparamref name="TValue2"/>.
    /// </summary>
    /// <param name="value">The value to conver.</param>
    /// <returns>The converted value of type <typeparamref name="TValue2"/>.</returns>
    TValue2 Convert(TValue1 value);
}
