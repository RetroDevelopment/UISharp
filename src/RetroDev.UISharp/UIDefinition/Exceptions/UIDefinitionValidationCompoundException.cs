using RetroDev.UISharp.Core.Exceptions;

namespace RetroDev.UISharp.UIDefinition.Exceptions;

/// <summary>
/// A list of <see cref="UIDefinitionValidationException"/> containing multiple causes of UIDefinition xml validation failure.
/// </summary>
public class UIDefinitionValidationCompoundException(IEnumerable<UIDefinitionValidationException> exceptions) : UIException($"UIDefinition validation failed with {exceptions.Count()} errors")
{
    /// <summary>
    /// The list of all <see cref="UIDefinitionValidationException"/> thrown during UIDefinition validation.
    /// </summary>
    public IEnumerable<UIDefinitionValidationException> Exceptions { get; } = exceptions;
}
