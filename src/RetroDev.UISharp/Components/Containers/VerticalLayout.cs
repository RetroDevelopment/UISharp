using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Containers;

/// <summary>
/// A layout displaying all elements vertically.
/// </summary>
public class VerticalLayout : UIContainer
{
    /// <summary>
    /// The list of layout cells.
    /// </summary>
    public UIPropertyCollection<Panel> Cells { get; }

    /// <summary>
    /// Creates a new vertical layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public VerticalLayout(Application application) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Cells = new UIPropertyCollection<Panel>(this);

        Cells.BindSourceToDestination(Items, item =>
        {
            var panel = new Panel(Application, item);
            panel.AutoHeight.Value = AutoSize.Wrap;
            return panel;
        });

        Children.BindSourceToDestination(Cells, panel => panel);
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        new(childrenSize.Max(s => s.Width) ?? PixelUnit.Zero,
            childrenSize.Sum(s => s.Height));

    /// <inheritdoc />
    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        var availableSpaceAfterPadding = availableSpace.Deflate(Padding.ToMarginStruct());
        var leftPadding = Padding.Left.Value.IfAuto(PixelUnit.Zero);
        var topPadding = Padding.Top.Value.IfAuto(PixelUnit.Zero);

        var verticalPosition = topPadding;
        List<Area?> childrenFinalSize = [];

        foreach (var childSize in childrenSize)
        {
            var x = leftPadding;
            var y = verticalPosition;
            var width = availableSpaceAfterPadding.Width;
            var height = childSize.Height;
            childrenFinalSize.Add(new Area(new Point(x, y), new Size(width, height)));
            verticalPosition += height;
        }

        return childrenFinalSize;
    }
}
