using RetroDev.UISharp.UIDefinition;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// Represents an automatic size (e.g., "*" or "auto").
/// </summary>
/// <remarks>
/// All auto cells will be equally distributed in size after applying the other constraints. For example,
/// given row constraints "100px;50%;auto;auto", assuming grid layout height of 1000px,
/// the last two rows will be equally distributed to occupy the remaining space (1000px - 100px - 50% = 400px) so
/// they will have 200px each.
/// </remarks>
[EAMLMatch(@"^(\*|auto)$")]
public record GridAutoSize : IGridSize
{
    /// <inheritdoc />
    public bool IsFixed => false;

    /// <summary>
    /// Invokes <see cref="IGridSizeVisitor{TValue}.Visit(GridAutoSize)"/> from the given <paramref name="visitor"/>.
    /// </summary>
    /// <typeparam name="TValue">The visitor return type.</typeparam>
    /// <param name="visitor">The visitor to invoke.</param>
    /// <returns>The value computed by the visitor.</returns>
    public TValue Accept<TValue>(IGridSizeVisitor<TValue> visitor) =>
        visitor.Visit(this);

}
