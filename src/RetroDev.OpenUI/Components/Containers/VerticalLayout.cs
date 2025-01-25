using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A layout displaying all elements vertically.
/// </summary>
public class VerticalLayout : Container, IContainer
{
    private readonly List<Panel> _panels = [];

    // TODO: once implementing the more complex SizeHint in gridLayout, just do _gridLayout.SizeHint;
    protected override Size ComputeSizeHint() => new(Children.Max(c => c.SizeEstimate.Width) ?? PixelUnit.Zero,
                                                     Children.Sum(c => c.SizeEstimate.Height));

    public override IEnumerable<UIComponent> Children => _panels.Select(p => p.Children.ElementAt(0));

    public IEnumerable<Panel> Panels => _panels;

    /// <summary>
    /// Creates a new vertical layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public VerticalLayout(Application application) : base(application, autoHeight: AutoSize.Wrap)
    {
    }

    public void AddComponent(UIComponent component)
    {
        AddComponent(component, null);
    }

    public void AddComponent(UIComponent component, UIComponent? after)
    {
        if (after != null)
        {
            var precedingPanelIndex = Children.ToList().FindIndex(c => c == after);
            if (precedingPanelIndex < 0) throw new ArgumentException("Vertical layout element not found");
            var panel = new Panel(Application);
            panel.SetComponent(component);
            AddChild(panel, precedingPanelIndex);
            _panels.Insert(precedingPanelIndex + 1, panel);
        }
        else
        {
            var panel = new Panel(Application);
            panel.SetComponent(component);
            _panels.Add(panel);
            AddChild(panel);
        }
    }

    public void RemoveComponent(uint index)
    {
        if (index >= _panels.Count) throw new ArgumentException($"Cannot remove vertical layout component with index {index}: the layout has only {_panels.Count} components");
        var panel = _panels[(int)index];
        RemoveChild(panel);
        _panels.Remove(panel);
    }

    public void Clear()
    {
        _panels.ForEach(p => RemoveChild(p));
        _panels.Clear();
    }

    protected override void RepositionChildren()
    {
        var verticalPosition = PixelUnit.Zero;

        foreach (var panel in _panels)
        {
            var child = panel.Children.First();
            panel.X.Value = 0;
            panel.Y.Value = verticalPosition;
            panel.Width.Value = RelativeDrawingArea.Size.Width;
            panel.Height.Value = child.SizeEstimate.Height;
            verticalPosition += panel.Height.Value;
        }
    }
}
