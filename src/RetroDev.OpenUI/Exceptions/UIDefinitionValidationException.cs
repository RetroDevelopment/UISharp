using RetroDev.OpenUI.UIDefinition.Ast;

namespace RetroDev.OpenUI.Exceptions;

/// <summary>
/// An exception that occurs when the UI definition validation fails.
/// </summary>
/// <param name="message">The error message.</param>
/// <param name="target">The affected AST <see cref="Node"/.></param>
/// <param name="innerException">The exception generating <see langword="this" /> exception, if any.</param>
public class UIDefinitionValidationException(string message, Node target, Exception? innerException = null) : UIException(message, innerException)
{
    /// <summary>
    /// The affected AST <see cref="Node"/>.
    /// </summary>
    public Node Target { get; } = target;
}
