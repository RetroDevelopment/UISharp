using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

public static class Helper
{
    /// <summary>
    /// Computes the total size required by size constraints that are calculated regardless of a specific cell optimal size.
    /// This method can be applied for height (in which case <paramref name="this"/> represents column constraints) and 
    /// width (in which case <paramref name="this"/> represents row constraints).
    /// </summary>
    /// <param name="this">The list of grid size constraints. If empty, it will be assumed that all size constraints are <see cref="GridAutoSize"/>.</param>
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
    /// <param name="this">The list of grid size constraints. If empty, it will be assumed that all size constraints are <see cref="GridAutoSize"/>.</param>
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

    /// <summary>
    /// Computes the optimal width or height of a grid layout.
    /// </summary>
    /// <param name="this">The list of grid size constraints. If empty, it will be assumed that all size constraints are <see cref="GridAutoSize"/>.</param>
    /// <param name="cellOptimalSizes">
    /// The rows or column optimal sizes.
    /// </param>
    /// <returns>
    /// The optimal width (if <paramref name="cellOptimalSizes"/> contain the optimal column sizes) or the optimal height
    /// (if <paramref name="cellOptimalSizes"/> contains the optimal row sizes).
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If <paramref name="this"/> list is not empty and <paramref name="cellOptimalSizes"/> and <paramref name="this"/> size are different.
    /// </exception>
    public static PixelUnit ComputeOptimalSize(this IEnumerable<IGridSize> @this, IEnumerable<PixelUnit> cellOptimalSizes)
    {
        var count = @this.Count();
        if (!@this.Any()) return (cellOptimalSizes.Max() ?? PixelUnit.Zero) * cellOptimalSizes.Count();
        if (cellOptimalSizes.Count() != count) throw new ArgumentException($"Cell count ({cellOptimalSizes.Count()}) and size constraint count ({count}) mismatch");

        var totalSize = PixelUnit.Zero;
        var relativeSizeConstraints = new List<(PixelUnit optimalSize, float percentage)>();
        var maxAutoSize = PixelUnit.Zero;
        var numberOfAutoSizeCells = 0;

        for (int i = 0; i < count; i++)
        {
            var sizeConstraint = @this.ElementAt(i);
            var cellSize = cellOptimalSizes.ElementAt(i);

            sizeConstraint.Accept(
                absoluteSize => totalSize += absoluteSize.Size,
                relativeSize => relativeSizeConstraints.Add((cellSize, relativeSize.NormalizedPercentage)), // Relative sizes
                autoSize => { maxAutoSize = Math.Max(maxAutoSize, cellSize); numberOfAutoSizeCells++; });
        }

        totalSize += maxAutoSize * numberOfAutoSizeCells;

        // Note that the calculation below is imprecise, it works up to 50% relative size constraint but for greater
        // relative sizes (e.g. 80%) it will sacrifice the space of auto size cells.
        // A perfect algorithm would be too complex and computationally expensive and the cost has yet to be proven useful.
        if (relativeSizeConstraints.Count > 0)
        {
            totalSize += relativeSizeConstraints.Sum(constraint => (totalSize + constraint.optimalSize) / constraint.percentage);
        }

        return totalSize;
    }

    /// <summary>
    /// Computes the optimal sizes for all rows and columns.
    /// </summary>
    /// <param name="this">The list of all cells optimal sizes in order <c>[row 0 column 0, row 0 column 1, ..., row 1 column 1, ...]</c></param>
    /// <param name="rows">The number of rows. It must be greater than zero.</param>
    /// <param name="columns">The number of columns. It must be greater than zero.</param>
    /// <returns>A pair where <c>rows</c> contains the list of optimal row heights and <c>columns</c> contains the list of optimal column widths.</returns>
    public static (IEnumerable<PixelUnit> rows, IEnumerable<PixelUnit> columns) ComputeOptimalCellSizes(this IEnumerable<Size> @this, uint rows, uint columns)
    {
        if (rows == 0) throw new ArgumentException("Row number cannot be zero");
        if (columns == 0) throw new ArgumentException("Column number cannot be zero");
        var count = rows * columns;
        if (count != @this.Count()) throw new ArgumentException($"Number of rows {rows} and columns {columns} does not match optimal sizes {@this.Count()}, expected {count} elements in list");
        var rowOptimalSizes = Enumerable.Repeat(PixelUnit.Zero, (int)rows).ToList();
        var columnsOptimalSizes = Enumerable.Repeat(PixelUnit.Zero, (int)columns).ToList();

        for (int i = 0; i < count; i++)
        {
            var cellSize = @this.ElementAt(i);
            var row = (int)(i / columns);
            var column = (int)(i % columns);

            rowOptimalSizes[row] = Math.Max(rowOptimalSizes[row], cellSize.Height);
            columnsOptimalSizes[column] = Math.Max(columnsOptimalSizes[column], cellSize.Width);
        }

        return (rowOptimalSizes, columnsOptimalSizes);
    }

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

    /// <summary>
    /// Visits <paramref name="this"/> <see cref="IGridSize"/>.
    /// </summary>
    /// <param name="visitGridAbsoluteSize">The function to visit for values of type <see cref="GridAbsoluteSize"/>.</param>
    /// <param name="visitGridRelativeSize">The function to visit for values of type <see cref="GridRelativeSize"/>.</param>
    /// <param name="visitGridAutoSize">The function to visit for values of type <see cref="GridAutoSize"/>.</param>  
    public static void Accept(this IGridSize @this,
                                        Action<GridAbsoluteSize> visitGridAbsoluteSize,
                                        Action<GridRelativeSize> visitGridRelativeSize,
                                        Action<GridAutoSize> visitGridAutoSize) =>
        @this.Accept(new LambdaGridSizeVisitor<object?>(
            absoluteSize => { visitGridAbsoluteSize(absoluteSize); return null; },
            relativeSize => { visitGridRelativeSize(relativeSize); return null; },
            autoSize => { visitGridAutoSize(autoSize); return null; }));

}
