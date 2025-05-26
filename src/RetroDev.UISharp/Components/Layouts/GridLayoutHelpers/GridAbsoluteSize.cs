using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.UIDefinition;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// Represents an absolute size in pixels (e.g., "100px", "100.123px").
/// </summary>
[EAMLMatch(@"^(?<size>\d+(\.\d+)?)px$")]
public record GridAbsoluteSize(PixelUnit Size) : IGridSize
{
    /// <inheritdoc />
    public bool IsFixed => true;

    /// <summary>
    /// Invokes <see cref="IGridSizeVisitor{TValue}.Visit(GridAbsoluteSize)"/> from the given <paramref name="visitor"/>.
    /// </summary>
    /// <typeparam name="TValue">The visitor return type.</typeparam>
    /// <param name="visitor">The visitor to invoke.</param>
    /// <returns>The value computed by the visitor.</returns>
    public TValue Accept<TValue>(IGridSizeVisitor<TValue> visitor) =>
        visitor.Visit(this);
}
