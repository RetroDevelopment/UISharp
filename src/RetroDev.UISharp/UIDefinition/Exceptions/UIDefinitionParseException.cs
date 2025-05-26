using RetroDev.UISharp.Core.Exceptions;
using RetroDev.UISharp.UIDefinition.Ast;

namespace RetroDev.UISharp.UIDefinition.Exceptions;

/// <summary>
/// An exception that occurs when the UI definition parse fails.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public class UIDefinitionParseException(string message, Exception? innerException = null) : UIException(message, innerException)
{
}
