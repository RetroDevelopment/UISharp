using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties.Exceptions;

/// <summary>
/// An exception that occurs when the validation of a UI property fails.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="target">The object where the property is located.</param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public class UIPropertyValidationException(string message, object? target = null, Exception? innerException = null) : UIException(CreateExceptionMessage(target, message), innerException)
{
    private static string CreateExceptionMessage(object? target, string message)
    {
        if (target == null) return message;

        if (target is UIComponent component)
        {
            return $"{message}; (target={target.GetType().FullName}, id={component.ID.Value})";
        }

        return $"{message} target={target.GetType().FullName}";
    }
}
