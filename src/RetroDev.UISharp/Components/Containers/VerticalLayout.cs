using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Containers;

/// <summary>
/// A layout displaying all elements vertically.
/// </summary>
public class VerticalLayout : UIContainer, IContainer
{
    private readonly List<Panel> _panels = [];

    public override IEnumerable<UIWidget> Children => _panels.Select(p => p.Children.ElementAt(0));

    public IEnumerable<Panel> Panels => _panels;

    /// <summary>
    /// Creates a new vertical layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public VerticalLayout(Application application) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
    }

    public void AddComponent(UIWidget component)
    {
        AddComponent(component, null);
    }

    public void AddComponent(UIWidget component, UIComponent? after)
    {
        if (after != null)
        {
            var precedingPanelIndex = Children.ToList().FindIndex(c => c == after);
            if (precedingPanelIndex < 0) throw new ArgumentException("Vertical layout element not found");
            var panel = new Panel(Application, component);
            panel.AutoHeight.Value = AutoSize.Wrap;
            AddChildNode(panel, precedingPanelIndex);
            _panels.Insert(precedingPanelIndex + 1, panel);
        }
        else
        {
            var panel = new Panel(Application, component);
            panel.AutoHeight.Value = AutoSize.Wrap;
            _panels.Add(panel);
            AddChildNode(panel);
        }
    }

    public void RemoveComponent(uint index)
    {
        if (index >= _panels.Count) throw new ArgumentException($"Cannot remove vertical layout component with index {index}: the layout has only {_panels.Count} components");
        var panel = _panels[(int)index];
        RemoveChildNode(panel);
        _panels.Remove(panel);
    }

    public void Clear()
    {
        _panels.ForEach(p => RemoveChildNode(p));
        _panels.Clear();
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
