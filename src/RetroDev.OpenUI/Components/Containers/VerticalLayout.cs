using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A layout displaying all elements vertically.
/// </summary>
public class VerticalLayout : Container, IContainer
{
    private readonly GridLayout _gridLayout;

    // TODO: once implementing the more complex SizeHint in gridLayout, just do _gridLayout.SizeHint;
    protected override Size ComputeSizeHint() => new(100, 100);

    // TODO: Use UIPropertyList to enable binding
    public UIProperty<VerticalLayout, string> RowSizes { get; }

    public override IEnumerable<UIComponent> Children => _gridLayout.Children;

    public IEnumerable<Panel> Panels => _gridLayout.Panels;

    /// <summary>
    /// Creates a new vertical layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public VerticalLayout(Application application) : base(application)
    {
        RowSizes = new UIProperty<VerticalLayout, string>(this, string.Empty);

        _gridLayout = new GridLayout(application);
        _gridLayout.Columns.Value = 1;
        _gridLayout.RowSizes.AddBinder(new PropertyBinder<VerticalLayout, string>(RowSizes, BindingType.DestinationToSource));

        AddChild(_gridLayout);
    }

    public void AddComponent(UIComponent component)
    {
        AddComponent(component, null);
    }

    public void AddComponent(UIComponent component, UIComponent? after)
    {
        _gridLayout.Rows.Value++;

        if (after != null)
        {
            var row = Children.ToList().FindIndex(c => c == after);
            if (row < 0) throw new ArgumentException("Vertical layout element not found");
            _gridLayout.AddComponent(component, row, 0);
        }
        else
        {
            _gridLayout.AddComponent(component);
        }
    }

    public void RemoveComponent(uint index)
    {
        if (index >= _gridLayout.Rows.Value) throw new ArgumentException($"Cannot remove vertical layout component with index {index}: the layout has only {_gridLayout.Rows.Value} components");
        _gridLayout.RemoveComponent(index, 0);
        _gridLayout.Rows.Value--;
    }

    public void Clear()
    {
        _gridLayout.Clear();
        _gridLayout.Columns.Value = 1;
        RowSizes.Value = string.Empty;
    }
}
