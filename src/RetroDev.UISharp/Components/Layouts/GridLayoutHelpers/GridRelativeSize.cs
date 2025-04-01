using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.UIDefinition;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// Represents a relative size as a percentage of the total available size (e.g., "1.01%").
/// </summary>
/// <remarks>
/// Note that the percentage is of total available space, without taking into account other layout constraints.
/// For example, if the layout height is 1000px, 50% for a row will 500px will be allocated.
/// </remarks>
[EAMLMatch(@"^(?<size>\d+(\.\d+)?)%$")]
public record GridRelativeSize(PixelUnit Size) : IGridSize
{
    /// <inheritdoc />
    public bool IsFixed => true;

    /// <summary>
    /// Gets the absolute size based of the given <paramref name="totalAvailableSpace"/>.
    /// </summary>
    /// <param name="totalAvailableSpace">The total available space (width or height) for a component.</param>
    /// <returns>The absolute size.</returns>
    public PixelUnit ToAbsolute(PixelUnit totalAvailableSpace) =>
        totalAvailableSpace * (this.Size / 100.0f);

    /// <summary>
    /// Invokes <see cref="IGridSizeVisitor{TValue}.Visit(GridRelativeSize)"/> from the given <paramref name="visitor"/>.
    /// </summary>
    /// <typeparam name="TValue">The visitor return type.</typeparam>
    /// <param name="visitor">The visitor to invoke.</param>
    /// <returns>The value computed by the visitor.</returns>
    public TValue Accept<TValue>(IGridSizeVisitor<TValue> visitor) =>
        visitor.Visit(this);
}
