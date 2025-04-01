using System.Net.Http.Headers;
using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// A size constraint on a grid.
/// </summary>
public interface IGridSize
{
    /// <summary>
    /// Whether the size is fixed and not calculated automatically based on whatever fits the children.
    /// </summary>
    bool IsFixed { get; }

    /// <summary>
    /// Invokes the corresponding visitor method, depending on <see langword="this" /> <see cref="IGridSize"/> implementation.
    /// </summary>
    /// <typeparam name="TValue">The visitor return type.</typeparam>
    /// <param name="visitor">The visitor to invoke.</param>
    /// <returns>The value computed by the visitor.</returns>
    TValue Accept<TValue>(IGridSizeVisitor<TValue> visitor);
}
