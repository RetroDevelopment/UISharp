namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// A <see cref="IGridSizeVisitor{TValue}"/> created from lambdas.
/// </summary>
/// <typeparam name="TValue"></typeparam>
/// <param name="visitGridAbsoluteSize">The function to visit for values of type <see cref="GridAbsoluteSize"/>.</param>
/// <param name="visitGridRelativeSize">The function to visit for values of type <see cref="GridRelativeSize"/>.</param>
/// <param name="visitGridAutoSize">The function to visit for values of type <see cref="GridAutoSize"/>.</param>
public class LambdaGridSizeVisitor<TValue>(Func<GridAbsoluteSize, TValue> visitGridAbsoluteSize,
                                           Func<GridRelativeSize, TValue> visitGridRelativeSize,
                                           Func<GridAutoSize, TValue> visitGridAutoSize) : IGridSizeVisitor<TValue>
{
    /// <inheritdoc />
    public TValue Visit(GridAbsoluteSize size) =>
        visitGridAbsoluteSize(size);

    /// <inheritdoc />
    public TValue Visit(GridRelativeSize size) =>
        visitGridRelativeSize(size);

    /// <inheritdoc />
    public TValue Visit(GridAutoSize size) =>
        visitGridAutoSize(size);
}
