using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Containers;

/// <summary>
/// A layout displaying all element in a row x column grid.
/// </summary>
public class GridLayout : UIContainer, IContainer
{
    public interface IGridSize { }
    public record AbsoluteSize(PixelUnit Size) : IGridSize;
    public record RelativeSize(float Size) : IGridSize;
    public record AutoSize : IGridSize;

    /// <summary>
    /// The number of layout rows.
    /// </summary>
    public UIProperty<GridLayout, uint> Rows { get; }

    /// <summary>
    /// The number of layout columns.
    /// </summary>
    public UIProperty<GridLayout, uint> Columns { get; }

    // TODO: Use UIPropertyList to enable binding
    // Example rowSizes="40px,*" columnSizes="auto,10.1%,*"
    public UIProperty<GridLayout, string> RowSizes { get; }

    // TODO: Use UIPropertyList to enable binding
    public UIProperty<GridLayout, string> ColumnSizes { get; }

    public override IEnumerable<UIWidget> Children => Panels.Select(c => c.Children.First());

    /// <summary>
    /// Gets the panel wrapping the layout children.
    /// </summary>
    public IEnumerable<Panel> Panels => GetChildrenNodes().Cast<Panel>();

    /// <summary>
    /// Creates a new grid layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="rows">The number of layout rows.</param>
    /// <param name="columns">The number of layout columns.</param>
    public GridLayout(Application application, uint rows = 0, uint columns = 0) : base(application)
    {
        Rows = new UIProperty<GridLayout, uint>(this, rows);
        Columns = new UIProperty<GridLayout, uint>(this, columns);
        RowSizes = new UIProperty<GridLayout, string>(this, string.Empty);
        ColumnSizes = new UIProperty<GridLayout, string>(this, string.Empty);
    }

    /// <summary>
    /// Adds a <see cref="UIWidget"/> to <see langword="this" /> layout.
    /// </summary>
    /// <param name="component">The component to add.</param>
    public void AddComponent(UIWidget component)
    {
        var panel = new Panel(Application, component);
        AddChildNode(panel);
    }

    /// <summary>
    /// Add a <see cref="UIWidget"/> to <see langword="this" /> layout in a specific position.
    /// </summary>
    /// <param name="component">The component to add.</param>
    /// <param name="row">The row where to position the component (zero-based index).</param>
    /// <param name="column">The column where to position the component (zero-based index).</param>
    /// <exception cref="ArgumentException">If <paramref name="row"/> and <paramref name="column"/> is not in the allowed range.</exception>
    public void AddComponent(UIWidget component, int row, int column)
    {
        var position = row * Columns.Value + column;
        var count = Children.Count();
        if (position > count) throw new ArgumentException($"Cannot insert element in grid layout at row {row} column {column}: the layout has only {count} components");

        var panel = new Panel(Application, component);
        AddChildNode(panel, (int)position);
    }

    /// <summary>
    /// Removes the component in the given <paramref name="row"/> and <paramref name="column"/>.
    /// </summary>
    /// <param name="row">The row location of the component to remove (zero-based index).</param>
    /// <param name="column">The column location of the component to remove (zero-based index).</param>
    /// <exception cref="ArgumentException">If the given <paramref name="row"/> and <paramref name="column"/> do not exist.</exception>
    public void RemoveComponent(uint row, uint column)
    {
        if (row >= Rows.Value) throw new ArgumentException($"Cannot remove component at row {row}, grid layout has only {Rows.Value} rows");
        if (column >= Columns.Value) throw new ArgumentException($"Cannot remove component at column {column}, grid layout has only {Columns.Value} columns");

        var indexOfElement = row * Columns.Value + column;
        var children = GetChildrenNodes();
        if (indexOfElement > children.Count()) throw new ArgumentException($"Cannot remove element at row {row} and column {column}: there are only {children.Count()} elements in the grid layout");
        var element = GetChildrenNodes().ElementAt((int)indexOfElement);
        RemoveChildNode(element);
    }

    /// <summary>
    /// Removes all the elements from <see langword="this" /> layout.
    /// </summary>
    public void Clear()
    {
        var count = Children.Count();

        for (int i = 0; i < count; i++)
        {
            RemoveChildNode(Panels.Last());
        }

        Rows.Value = 0;
        Columns.Value = 0;
        RowSizes.Value = string.Empty;
        ColumnSizes.Value = string.Empty;
    }

    /// <inheritdoc />
    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        EnsureRowsColumnFitNumberOfChildren();

        if (availableSpace == Size.Zero) return Enumerable.Repeat<Area?>(Area.Empty, childrenSize.Count()).ToList();

        var areas = new List<Area?>();

        var availableSpaceAfterPadding = availableSpace.Deflate(Padding.ToMarginStruct());
        var leftPadding = Padding.Left.Value.IfAuto(PixelUnit.Zero);
        var topPadding = Padding.Top.Value.IfAuto(PixelUnit.Zero);

        var rowSizeDefinitions = Parse(RowSizes.Value, Rows.Value);
        var columnSizeDefinitions = Parse(ColumnSizes.Value, Columns.Value);
        var rowSizes = ComputeSizes(availableSpaceAfterPadding.Height, rowSizeDefinitions, Rows.Value);
        var columnSizes = ComputeSizes(availableSpaceAfterPadding.Width, columnSizeDefinitions, Columns.Value);

        var children = base.GetChildrenNodes();
        var size = children.Count();
        var i = 0u;

        // TODO: no need to iterate over children.
        foreach (var child in children)
        {
            var row = i / Columns.Value;
            var column = i % Columns.Value;

            var width = columnSizes[(int)column];
            var height = rowSizes[(int)row];
            var x = column == 0 ? 0.0f : columnSizes[..((int)column)].Sum(p => p.Value);
            var y = row == 0 ? 0.0f : rowSizes[..((int)row)].Sum(p => p.Value);

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
        var columnSizeDefinitions = Parse(ColumnSizes.Value, Columns.Value);
        var rowSizeDefinitions = Parse(RowSizes.Value, Rows.Value);
        var autoColumnCells = 0;
        var maximumColumnWidth = PixelUnit.Zero;
        var cumulativeFixedWidth = PixelUnit.Zero;
        var autoRowCells = 0;
        var maximumRowHeight = PixelUnit.Zero;
        var cumulativeFixedHeight = PixelUnit.Zero;
        int index = 0;

        foreach (var column in columnSizeDefinitions)
        {
            if (column is AutoSize || column is RelativeSize)
            {
                for (int rowIndex = 0; rowIndex < Rows.Value; rowIndex++)
                {
                    var childIndex = rowIndex * (int)Columns.Value + index;
                    if (childIndex >= childrenSizeList.Count) break;
                    maximumColumnWidth = Math.Max(maximumColumnWidth, childrenSizeList[childIndex].Width);
                }

                autoColumnCells++;
            }
            else if (column is AbsoluteSize size)
            {
                cumulativeFixedWidth += size.Size;
            }

            index++;
        }

        index = 0;

        foreach (var row in rowSizeDefinitions)
        {
            if (row is AutoSize || row is RelativeSize)
            {
                for (int columnIndex = 0; columnIndex < Columns.Value; columnIndex++)
                {
                    var childIndex = index * (int)Columns.Value + columnIndex;
                    if (childIndex >= childrenSizeList.Count) break;
                    maximumRowHeight = Math.Max(maximumRowHeight, childrenSizeList[childIndex].Height);
                }

                autoRowCells++;
            }
            else if (row is AbsoluteSize size)
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
        var numberOfItems = GetChildrenNodes().Count();
        var maximumNumberOfItems = Rows.Value * Columns.Value;

        if (numberOfItems > maximumNumberOfItems)
        {
            throw new InvalidOperationException($"Grid layout with {Rows.Value} rows and {Columns.Value} columns can contain at most {maximumNumberOfItems} items, but {numberOfItems} provided");
        }
    }

    private static List<IGridSize> Parse(string sizeExpression, uint count)
    {
        if (sizeExpression == string.Empty)
        {
            return Enumerable.Repeat(new AutoSize(), (int)count).ToList<IGridSize>();
        }

        var result = new List<IGridSize>();
        var sizeList = sizeExpression.Replace(';', ',').Split(",");

        foreach (var size in sizeList)
        {
            if (size.EndsWith("px"))
            {
                var parseSuccess = float.TryParse(size.Substring(0, size.Length - 2), null, out var sizeNumber);
                if (!parseSuccess) throw new InvalidOperationException($"Failed to convert {size} as number");
                result.Add(new AbsoluteSize(sizeNumber));
            }
            else if (size.EndsWith('%'))
            {
                var parseSuccess = float.TryParse(size.Substring(0, size.Length - 1), null, out var sizeNumber);
                if (!parseSuccess) throw new InvalidOperationException($"Failed to convert {size} as number");
                if (sizeNumber > 100 || sizeNumber < 0) throw new InvalidOperationException($"Invalid size {size}: relative values must be between 0% to 100%");
                result.Add(new RelativeSize(sizeNumber / 100.0f));
            }
            else if (size == "auto" || size == "*")
            {
                result.Add(new AutoSize());
            }
            else
            {
                throw new InvalidOperationException($"Invalid auto size formula {size}");
            }
        }

        return result;
    }

    private List<PixelUnit> ComputeSizes(PixelUnit maximumSize, List<IGridSize> sizes, uint count)
    {
        var result = new List<PixelUnit>();

        var cumulativeKnownSize = ComputeCumulativeKnownSize(maximumSize, sizes);
        if (cumulativeKnownSize > maximumSize) throw new InvalidOperationException($"Cumulative size exceeds the maximum layout size of {maximumSize}");
        var numberOfAutoSizeElements = sizes.Where(s => s is AutoSize).Count();
        // Use ceiling to calculate grid size because sometimes rounding errors might create 1 pixel gap between grid layout components.
        var autoGridSize = (float)Math.Ceiling((maximumSize - cumulativeKnownSize) / numberOfAutoSizeElements);

        foreach (var size in sizes)
        {
            if (size is AbsoluteSize absoluteSize) result.Add(absoluteSize.Size);
            else if (size is RelativeSize relateiveSize) result.Add(maximumSize * relateiveSize.Size);
            else if (size is AutoSize autoSize) result.Add(autoGridSize);
            else throw new InvalidOperationException($"Unhandled grid layout autosize type {size.GetType()}");
        }

        if (result.Count != count) throw new InvalidOperationException($"Grid layout size pattern must be {count} but {result.Count} elements found");
        return result;
    }

    private PixelUnit ComputeCumulativeKnownSize(PixelUnit maximumSize, List<IGridSize> sizes)
    {
        PixelUnit cumulativeKnownSize = 0.0f;

        foreach (var size in sizes)
        {
            if (size is AbsoluteSize absoluteSize) cumulativeKnownSize += absoluteSize.Size;
            if (size is RelativeSize relateiveSize) cumulativeKnownSize += maximumSize * relateiveSize.Size;
        }

        return cumulativeKnownSize;
    }
}
