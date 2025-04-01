using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

public static class Helper
{
    /// <summary>
    /// Computes the total size required by size constraints that are calculated regardless of a specific cell optimal size.
    /// This method can be applied for height (in which case <paramref name="this"/> represents column constraints) and 
    /// width (in which case <paramref name="this"/> represents row constraints).
    /// </summary>
    /// <param name="this">The list of grid size constraints.</param>
    /// <param name="totalAvailableSize">The total available size for a layout.</param>
    /// <returns>
    /// The total size required by size constraints that are calculated regardless of a specific cell optimal size, or
    /// <see cref="PixelUnit.Zero"/> if no constraint is specified.
    /// </returns>
    public static PixelUnit ComputeTotalFixedSize(this IEnumerable<IGridSize> @this, PixelUnit totalAvailableSize)
    {
        if (!@this.Any()) return PixelUnit.Zero;
        var fixedSize = PixelUnit.Zero;

        foreach (var size in @this)
        {
            fixedSize += size.Accept<PixelUnit>(absoluteSize => absoluteSize.Size,
                                                relativeSize => relativeSize.ToAbsolute(totalAvailableSize),
                                                _ => PixelUnit.Zero);
        }

        return fixedSize;
    }

    /// <summary>
    /// Computes the size for each row or column based of <paramref name="this"/> size constraints.
    /// This method can be applied for height (in which case <paramref name="this"/> represents column constraints) and 
    /// width (in which case <paramref name="this"/> represents row constraints).
    /// </summary>
    /// <param name="this">The list of grid size constraints.</param>
    /// <param name="totalAvailableSize">The total available size for a layout.</param>
    /// <param name="numberOfCells">The total number of cells.</param>
    /// <returns>The list where an element at index <c>i</c> represents the size (width or height) of the <c>i</c>-th column or row respectively.</returns>
    public static IEnumerable<PixelUnit> ComputeSizes(this IEnumerable<IGridSize> @this, PixelUnit totalAvailableSize, uint numberOfCells)
    {
        if (!@this.Any()) return Enumerable.Repeat<PixelUnit>(totalAvailableSize / numberOfCells, (int)numberOfCells);
        var totalFixedSize = @this.ComputeTotalFixedSize(totalAvailableSize);
        var exceedingFactor = Math.Clamp(totalFixedSize / totalAvailableSize, 1.0f, PixelUnit.Max);
        var totalNonFixedSize = (totalAvailableSize - totalFixedSize / exceedingFactor);
        var numberOfAutoSizeCells = @this.Count(size => !size.IsFixed);
        var sizePerCell = totalNonFixedSize / numberOfAutoSizeCells;
        List<PixelUnit> sizeList = [];

        foreach (var size in @this)
        {
            var cellSize = size.Accept<PixelUnit>(absoluteSize => absoluteSize.Size / exceedingFactor,
                                                  relativeSize => relativeSize.ToAbsolute(totalAvailableSize) / exceedingFactor,
                                                  _ => sizePerCell);
            sizeList.Add(Math.Clamp(cellSize, PixelUnit.Zero, totalAvailableSize));
        }

        return sizeList;
    }

    // Optimal sizes
    public static IEnumerable<PixelUnit> ComputeOptimalSizes(this IEnumerable<IGridSize> @this, PixelUnit maximumCellSize) => throw new NotImplementedException();

    /// <summary>
    /// Visits <paramref name="this"/> <see cref="IGridSize"/>.
    /// </summary>
    /// <typeparam name="TValue">The visiting return type.</typeparam>
    /// <param name="visitGridAbsoluteSize">The function to visit for values of type <see cref="GridAbsoluteSize"/>.</param>
    /// <param name="visitGridRelativeSize">The function to visit for values of type <see cref="GridRelativeSize"/>.</param>
    /// <param name="visitGridAutoSize">The function to visit for values of type <see cref="GridAutoSize"/>.</param>  
    /// <returns>The visitor return value.</returns>
    public static TValue Accept<TValue>(this IGridSize @this,
                                        Func<GridAbsoluteSize, TValue> visitGridAbsoluteSize,
                                        Func<GridRelativeSize, TValue> visitGridRelativeSize,
                                        Func<GridAutoSize, TValue> visitGridAutoSize) =>
        @this.Accept(new LambdaGridSizeVisitor<TValue>(visitGridAbsoluteSize, visitGridRelativeSize, visitGridAutoSize));
}
