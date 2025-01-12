using System.Diagnostics.Metrics;
using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

// TODO: implement select with cursor
// TODO: scroll view configuration
// TODO: implement multiple selection

/// <summary>
/// Provides a vertical list of components and a logic to select components.
/// </summary>
/// <remarks>
/// Although list boxes are typically used to list text in order to allow selecting one or more options,
/// the <see cref="ListBox"/> class allows to list not only text but any <see cref="UIComponent"/>.
/// </remarks>
public class ListBox : Container, IContainer
{
    private readonly VerticalLayout _verticalLayout;
    private readonly ScrollView _scrollView;

    protected override Size ComputeSizeHint() => new(100, 100);

    /// <summary>
    /// The index of the selected element in the list, or <see langword="null" /> if no element is selected.
    /// </summary>
    public UIProperty<ListBox, uint?> SelectedIndex { get; }

    /// <summary>
    /// The selected element in the list, or <see langword="null" /> if no element is selected.
    /// </summary>
    public UIProperty<ListBox, UIComponent?> SelectedItem { get; }

    public override IEnumerable<UIComponent> Children => _verticalLayout.Children;

    public ListBox(Application application) : base(application)
    {
        _verticalLayout = new VerticalLayout(application);
        _scrollView = new ScrollView(application);

        _scrollView.SetComponent(_verticalLayout);
        _verticalLayout.AutoWidth.Value = AutoSizeStrategy.WrapComponentLeftTop;
        _verticalLayout.AutoHeight.Value = AutoSizeStrategy.WrapComponentLeftTop;

        SelectedIndex = new UIProperty<ListBox, uint?>(this, null);
        SelectedIndex.ValueChange += SelectedIndex_ValueChange;

        SelectedItem = new UIProperty<ListBox, UIComponent?>(this, null);
        SelectedItem.ValueChange += SelectedItem_ValueChange;

        AddChild(_scrollView);
    }

    public void AddComponent(UIComponent component)
    {
        AddComponent(component, null);
    }

    public void AddComponent(UIComponent component, UIComponent? after)
    {
        _verticalLayout.AddComponent(component, after);
        var container = _verticalLayout.Panels.First(p => p.Children.ElementAt(0) == component);
        container.MousePress += Container_MousePress;
        container.ChildrenRendered += Container_ChildrenRendered;
    }

    public void RemoveComponent(uint index)
    {
        SelectedIndex.Value = null;
        _verticalLayout.RowSizes.Value = string.Empty;
        _verticalLayout.Width.Value = float.PositiveInfinity;
        _verticalLayout.Height.Value = float.PositiveInfinity;
        _verticalLayout.RemoveComponent(index);
    }

    public void Clear()
    {
        _verticalLayout.Clear();
        SelectedIndex.Value = null;
    }

    private void Container_ChildrenRendered(UIComponent sender, Events.RenderingEventArgs e)
    {
        if (((Panel)sender).Children.ElementAt(0) == SelectedItem.Value)
        {
            var rectangle = new Rectangle(new Color(255, 0, 0, 100));
            var size = sender.RelativeDrawingArea.Size;
            e.Canvas.Render(rectangle, new(Point.Zero, size));
        }
    }

    private void Container_MousePress(UIComponent sender, Events.MouseEventArgs e)
    {
        var selectedPanel = (Panel)sender;
        var index = _verticalLayout.Panels.ToList().IndexOf(selectedPanel);
        if (index < 0) throw new ArgumentException($"Cannot find element in list box: make sure the element has not been deleted");
        SelectedIndex.Value = (uint)index;
    }

    private void SelectedIndex_ValueChange(ListBox sender, ValueChangeEventArgs<uint?> e)
    {
        var numberOfItems = _verticalLayout.Children.Count();
        if (e.CurrentValue != null && e.CurrentValue.Value > numberOfItems)
        {
            throw new InvalidOperationException($"Cannot select ListBox item with index {e.CurrentValue}: the ListBox has {numberOfItems} elements");
        }

        SelectedItem.Value = e.CurrentValue != null ? Children.ElementAt((int)e.CurrentValue) : null;
    }

    private void SelectedItem_ValueChange(ListBox sender, ValueChangeEventArgs<UIComponent?> e)
    {
        if (e.CurrentValue == null)
        {
            SelectedIndex.Value = null;
            return;
        }

        var selectedIndex = _verticalLayout.Children.ToList().IndexOf(e.CurrentValue);
        if (selectedIndex == -1) throw new InvalidOperationException("ListBox selected element not found");
        SelectedIndex.Value = (uint)selectedIndex;
    }

    protected override void RepositionChildrenImplementation()
    {
        if (!Children.Any()) return;

        _scrollView.Width.Value = RelativeDrawingArea.Size.Width;
        _scrollView.Height.Value = RelativeDrawingArea.Size.Height;

        _verticalLayout.RowSizes.Value = string.Join(',', Children.Select(c => c.RelativeDrawingArea.Size.Height.Value.ToString() + "px"));
        _verticalLayout.Width.Value = Math.Max(Children.Max(c => c.RelativeDrawingArea.Size.Width.Value), RelativeDrawingArea.Size.Width);
        _verticalLayout.Height.Value = Children.Sum(c => c.RelativeDrawingArea.Size.Height.Value);

        var verticalLayoutSize = _verticalLayout.RelativeDrawingArea.Size;
        var scrollViewSize = _scrollView.RelativeDrawingArea.Size;

        if (verticalLayoutSize.Width < scrollViewSize.Width || verticalLayoutSize.Height < scrollViewSize.Height)
        {
            _verticalLayout.X.Value = 0;
            _verticalLayout.Y.Value = 0;
        }
    }
}
