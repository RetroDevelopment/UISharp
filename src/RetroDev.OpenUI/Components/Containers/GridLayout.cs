using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A layout displaying all element in a row x column grid.
/// </summary>
public class GridLayout : Container, IContainer
{
    public interface IGridSize { }
    public record AbsoluteSize(PixelUnit Size) : IGridSize;
    public record RelateiveSize(float Size) : IGridSize;
    public record AutoSize : IGridSize;

    // TODO: Smart auto size that fits exactly all children
    protected override Size ComputeSizeHint() => new(100, 100);

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

    public override IEnumerable<UIComponent> Children => GetChildren().Cast<Panel>().Select(c => c.Children.First());

    /// <summary>
    /// Gets the panel wrapping the layout children.
    /// </summary>
    public IEnumerable<Panel> Panels => GetChildren().Cast<Panel>();

    /// <summary>
    /// Creates a new grid layout.
    /// </summary>
    /// <param name="parent">The application owning this component.</param>
    public GridLayout(Application parent) : base(parent)
    {
        Rows = new UIProperty<GridLayout, uint>(this, 0);
        Columns = new UIProperty<GridLayout, uint>(this, 0);
        RowSizes = new UIProperty<GridLayout, string>(this, string.Empty);
        ColumnSizes = new UIProperty<GridLayout, string>(this, string.Empty);
    }

    public void AddComponent(UIComponent component)
    {
        var panel = new Panel(Application);
        panel.SetComponent(component);
        AddChild(panel);
    }

    public void AddComponent(UIComponent component, int row, int column)
    {
        var position = row * Columns.Value + column;
        var count = Children.Count();
        if (position > count) throw new ArgumentException($"Cannot insert element in grid layout at row {row} column {column}: the layout has only {count} components");

        var panel = new Panel(Application);
        panel.SetComponent(component);
        AddChild(panel, (int)position);
    }

    public void RemoveComponent(uint row, uint column)
    {
        if (row >= Rows) throw new ArgumentException($"Cannot remove component at row {row}, grid layout has only {Rows.Value} rows");
        if (column >= Columns) throw new ArgumentException($"Cannot remove component at column {column}, grid layout has only {Columns.Value} columns");

        var indexOfElement = row * Columns + column;
        var children = GetChildren();
        if (indexOfElement > children.Count()) throw new ArgumentException($"Cannot remove element at row {row} and column {column}: there are only {children.Count()} elements in the grid layout");
        var element = GetChildren().ElementAt((int)indexOfElement);
        RemoveChild(element);
    }

    public void Clear()
    {
        var count = Children.Count();

        for (int i = 0; i < count; i++)
        {
            RemoveChild(Panels.Last());
        }

        Rows.Value = 0;
        Columns.Value = 0;
        RowSizes.Value = string.Empty;
        ColumnSizes.Value = string.Empty;
    }

    protected override void RepositionChildrenImplementation()
    {
        EnsureRowsColumnFitNumberOfChildren();

        var layoutSize = RelativeDrawingArea.Size;

        var rowSizeDefinitions = Parse(RowSizes.Value, Rows);
        var columnSizeDefinitions = Parse(ColumnSizes.Value, Columns);
        var rowSizes = ComputeSizes(layoutSize.Height, rowSizeDefinitions, Rows);
        var columnSizes = ComputeSizes(layoutSize.Width, columnSizeDefinitions, Columns);

        var children = base.GetChildren();
        var size = children.Count();
        var i = 0u;

        foreach (var child in children)
        {
            var row = i / Columns;
            var column = i % Columns;
            var cell = (Panel)child;

            cell.Width.Value = columnSizes[(int)column];
            cell.Height.Value = rowSizes[(int)row];
            cell.X.Value = column == 0 ? 0.0f : columnSizes[..((int)column)].Sum(p => p.Value);
            cell.Y.Value = row == 0 ? 0.0f : rowSizes[..((int)row)].Sum(p => p.Value);

            i++;
        }
    }

    private void EnsureRowsColumnFitNumberOfChildren()
    {
        var numberOfItems = GetChildren().Count();
        var maximumNumberOfItems = Rows * Columns;

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
                result.Add(new RelateiveSize(sizeNumber / 100.0f));
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
            else if (size is RelateiveSize relateiveSize) result.Add(maximumSize * relateiveSize.Size);
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
            if (size is RelateiveSize relateiveSize) cumulativeKnownSize += maximumSize * relateiveSize.Size;
        }

        return cumulativeKnownSize;
    }
}
