using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Exceptions;

/// <summary>
/// An exception that occurs when the validation of a UI property fails.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public class UIPropertyValidationException(string message, Exception? innerException = null) : UIException(message, innerException)
{
    public static void EnsureGreaterOrEqualTo<TParent, TValue>(UIProperty<TParent, TValue> property, TValue minimumValue)
    {
        if (property.Value < minimumValue)
        {
            throw new UIPropertyValidationException($"{propertyName} must be greater or equal to {min}");
        }
    }
}
