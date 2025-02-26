using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Windowing.Events;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A layout displaying all elements vertically.
/// </summary>
public class VerticalLayout : UIContainer, IContainer
{
    private readonly List<Panel> _panels = [];

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        new(childrenSize.Max(s => s.Width) ?? PixelUnit.Zero,
            childrenSize.Sum(s => s.Height));

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
            var panel = new Panel(Application);
            panel.AutoHeight.Value = AutoSize.Wrap;
            panel.SetComponent(component);
            AddChildNode(panel, precedingPanelIndex);
            _panels.Insert(precedingPanelIndex + 1, panel);
        }
        else
        {
            var panel = new Panel(Application);
            panel.AutoHeight.Value = AutoSize.Wrap;
            panel.SetComponent(component);
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

    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        var verticalPosition = PixelUnit.Zero;
        List<Area?> childrenFinalSize = [];

        foreach (var childSize in childrenSize)
        {
            var x = 0;
            var y = verticalPosition;
            var width = availableSpace.Width;
            var height = childSize.Height;
            childrenFinalSize.Add(new Area(new Point(x, y), new Size(width, height)));
            verticalPosition += height;
        }

        return childrenFinalSize;
    }
}
