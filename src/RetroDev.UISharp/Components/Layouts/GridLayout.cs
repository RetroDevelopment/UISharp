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
    public override void Validate()
    {
        base.Validate();
        var numberOfItems = Children.Count;
        var maximumNumberOfItems = Rows.Value * Columns.Value;

        if (numberOfItems > maximumNumberOfItems)
        {
            throw new InvalidOperationException($"Grid layout with {Rows.Value} rows and {Columns.Value} columns can contain at most {maximumNumberOfItems} items, but {numberOfItems} provided");
        }
    }

    /// <inheritdoc />
    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        var areas = new List<Area?>();

        var availableSpaceAfterPadding = availableSpace.Deflate(Padding.ToMarginStruct());
        var leftPadding = Padding.Left.Value.IfAuto(PixelUnit.Zero);
        var topPadding = Padding.Top.Value.IfAuto(PixelUnit.Zero);

        var rowSizes = RowSizes.ComputeSizes(availableSpaceAfterPadding.Height, Rows.Value);
        var columnSizes = ColumnSizes.ComputeSizes(availableSpaceAfterPadding.Width, Columns.Value);

        var size = childrenSize.Count();
        var i = 0;

        foreach (var child in childrenSize)
        {
            var row = (int)(i / Columns.Value);
            var column = (int)(i % Columns.Value);

            var width = columnSizes.ElementAt(column);
            var height = rowSizes.ElementAt(row);
            var x = columnSizes.Take(column).Sum(p => p.Value);
            var y = rowSizes.Take(row).Sum(p => p.Value);

            areas.Add(new Area(new Point(x + leftPadding, y + topPadding), new Size(width, height)));
            i++;
        }

        return areas;
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        (var rowSizes, var columnSizes) = childrenSize.ComputeOptimalCellSizes(Rows.Value, Columns.Value);
        var optimalWidth = ColumnSizes.ComputeOptimalSize(columnSizes);
        var optimalHeight = RowSizes.ComputeOptimalSize(rowSizes);
        return new Size(optimalWidth, optimalHeight);
    }
}
