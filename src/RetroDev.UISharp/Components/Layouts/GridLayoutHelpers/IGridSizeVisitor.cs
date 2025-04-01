namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// The visitor for <see cref="IGridSize"/>.
/// </summary>
/// <typeparam name="TValue">The visitor return type.</typeparam>
public interface IGridSizeVisitor<TValue>
{
    /// <summary>
    /// Visits the given <paramref name="size"/>.
    /// </summary>
    /// <param name="size">The size to visit.</param>
    /// <returns>The visitor return value.</returns>
    TValue Visit(GridAbsoluteSize size);

    /// <summary>
    /// Visits the given <paramref name="size"/>.
    /// </summary>
    /// <param name="size">The size to visit.</param>
    /// <returns>The visitor return value.</returns>
    TValue Visit(GridRelativeSize size);

    /// <summary>
    /// Visits the given <paramref name="size"/>.
    /// </summary>
    /// <param name="size">The size to visit.</param>
    /// <returns>The visitor return value.</returns>
    TValue Visit(GridAutoSize size);
}
