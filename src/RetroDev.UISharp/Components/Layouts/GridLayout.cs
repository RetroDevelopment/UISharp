using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Layouts;

/// <summary>
/// A layout displaying all element in a row x column grid.
/// </summary>
public class GridLayout : UIContainer
{
    /// <summary>
    /// The number of layout rows.
    /// </summary>
    public UIProperty<uint> Rows { get; }

    /// <summary>
    /// The number of layout columns.
    /// </summary>
    public UIProperty<uint> Columns { get; }

    /// <summary>
    /// A list of row size constraints.
    /// </summary>
    public UIPropertyCollection<IGridSize> RowSizes { get; }

    /// <summary>
    /// A list of column size constraint.
    /// </summary>
    public UIPropertyCollection<IGridSize> ColumnSizes { get; }

    /// <summary>
    /// Creates a new grid layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="rows">The number of layout rows.</param>
    /// <param name="columns">The number of layout columns.</param>
    public GridLayout(Application application, uint rows = 0, uint columns = 0) : base(application)
    {
        Rows = new UIProperty<uint>(this, rows);
        Columns = new UIProperty<uint>(this, columns);
        RowSizes = new UIPropertyCollection<IGridSize>(this);
        ColumnSizes = new UIPropertyCollection<IGridSize>(this);

        Children.BindSourceToDestination(Items, item => new Panel(Application, item));
    }

    /// <inheritdoc />
    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        EnsureRowsColumnFitNumberOfChildren();

        var areas = new List<Area?>();

        var availableSpaceAfterPadding = availableSpace.Deflate(Padding.ToMarginStruct());
        var leftPadding = Padding.Left.Value.IfAuto(PixelUnit.Zero);
        var topPadding = Padding.Top.Value.IfAuto(PixelUnit.Zero);

        var rowSizes = RowSizes.ComputeSizes(availableSpaceAfterPadding.Height, Rows.Value);
        var columnSizes = ColumnSizes.ComputeSizes(availableSpaceAfterPadding.Width, Columns.Value);

        var size = childrenSize.Count();
        var i = 0u;

        foreach (var child in childrenSize)
        {
            var row = i / Columns.Value;
            var column = i % Columns.Value;

            var width = columnSizes.ElementAt((int)column);
            var height = rowSizes.ElementAt((int)row);
            var x = column == 0 ? 0.0f : columnSizes.Take((int)column).Sum(p => p.Value);
            var y = row == 0 ? 0.0f : rowSizes.Take((int)row).Sum(p => p.Value);

            areas.Add(new Area(new Point(x + leftPadding, y + topPadding), new Size(width, height)));
            i++;
        }

        return areas;
    }

    // TODO: Smart auto size that fits exactly all children
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        // TODO: refactor and take into account relative width and height
        var childrenSizeList = childrenSize.ToList();
        var columnSizeDefinitions = ColumnSizes.Any() ? ColumnSizes.ToList() : Enumerable.Repeat(new GridAutoSize(), (int)Columns.Value).ToList<IGridSize>();
        var rowSizeDefinitions = RowSizes.Any() ? RowSizes.ToList() : Enumerable.Repeat(new GridAutoSize(), (int)Rows.Value).ToList<IGridSize>();
        var autoColumnCells = 0;
        var maximumColumnWidth = PixelUnit.Zero;
        var cumulativeFixedWidth = PixelUnit.Zero;
        var autoRowCells = 0;
        var maximumRowHeight = PixelUnit.Zero;
        var cumulativeFixedHeight = PixelUnit.Zero;
        var index = 0;

        foreach (var column in columnSizeDefinitions)
        {
            if (column is GridAutoSize || column is GridRelativeSize)
            {
                for (var rowIndex = 0; rowIndex < Rows.Value; rowIndex++)
                {
                    var childIndex = rowIndex * (int)Columns.Value + index;
                    if (childIndex >= childrenSizeList.Count) break;
                    maximumColumnWidth = Math.Max(maximumColumnWidth, childrenSizeList[childIndex].Width);
                }

                autoColumnCells++;
            }
            else if (column is GridAbsoluteSize size)
            {
                cumulativeFixedWidth += size.Size;
            }

            index++;
        }

        index = 0;

        foreach (var row in rowSizeDefinitions)
        {
            if (row is GridAutoSize || row is GridRelativeSize)
            {
                for (var columnIndex = 0; columnIndex < Columns.Value; columnIndex++)
                {
                    var childIndex = index * (int)Columns.Value + columnIndex;
                    if (childIndex >= childrenSizeList.Count) break;
                    maximumRowHeight = Math.Max(maximumRowHeight, childrenSizeList[childIndex].Height);
                }

                autoRowCells++;
            }
            else if (row is GridAbsoluteSize size)
            {
                cumulativeFixedHeight += size.Size;
            }

            index++;
        }

        var optimalCellWidth = maximumColumnWidth * autoColumnCells + cumulativeFixedWidth;
        var optimalCellHeight = maximumRowHeight * autoRowCells + cumulativeFixedHeight;
        return new Size(optimalCellWidth, optimalCellHeight);
    }

    private void EnsureRowsColumnFitNumberOfChildren()
    {
        var numberOfItems = Children.Count;
        var maximumNumberOfItems = Rows.Value * Columns.Value;

        if (numberOfItems > maximumNumberOfItems)
        {
            throw new InvalidOperationException($"Grid layout with {Rows.Value} rows and {Columns.Value} columns can contain at most {maximumNumberOfItems} items, but {numberOfItems} provided");
        }
    }
}
