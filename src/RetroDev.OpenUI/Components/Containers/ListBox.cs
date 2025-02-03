using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
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

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.Any() ? childrenSize.First() : Size.Zero;

    /// <summary>
    /// The index of the selected element in the list, or <see langword="null" /> if no element is selected.
    /// </summary>
    public UIProperty<ListBox, uint?> SelectedIndex { get; }

    /// <summary>
    /// The selected element in the list, or <see langword="null" /> if no element is selected.
    /// </summary>
    public UIProperty<ListBox, UIComponent?> SelectedItem { get; }

    /// <inheritdoc />
    public override IEnumerable<UIComponent> Children => _verticalLayout.Children;

    /// <summary>
    /// Creates a new grid layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public ListBox(Application application) : base(application)
    {
        _verticalLayout = new VerticalLayout(application);
        _scrollView = new ScrollView(application);

        _scrollView.SetComponent(_verticalLayout);
        _scrollView.AutoWidth.Value = AutoSize.Stretch;
        _scrollView.AutoHeight.Value = AutoSize.Stretch;
        // TODO: add property so user can decide if auto width and height are stretch or wrap for vertical layout.
        // This changes how the list box is displayed. Also add whether to center or do top left, etc. for inner vertical layout components.
        // What about adding a new auto size like "MaxWrapStretch" that takes maximum size between wrap and stretch?
        // That is how many list box look like: they occupy at least the layout plus some more.
        _verticalLayout.AutoWidth.Value = AutoSize.Stretch;

        SelectedIndex = new UIProperty<ListBox, uint?>(this, (uint?)null);
        SelectedIndex.ValueChange += SelectedIndex_ValueChange;

        SelectedItem = new UIProperty<ListBox, UIComponent?>(this, (UIComponent?)null);
        SelectedItem.ValueChange += SelectedItem_ValueChange;

        AddChild(_scrollView);
    }

    public void AddComponent(UIComponent component)
    {
        AddComponent(component, null);
    }

    public void AddComponent(UIComponent component, UIComponent? after = null)
    {
        _verticalLayout.AddComponent(component, after);
        var container = _verticalLayout.Panels.First(p => p.Children.ElementAt(0) == component);
        container.MousePress += Container_MousePress;
    }

    public void RemoveComponent(uint index)
    {
        SelectedIndex.Value = null;
        _verticalLayout.RemoveComponent(index);
    }

    public void Clear()
    {
        _verticalLayout.Clear();
        SelectedIndex.Value = null;
    }

    private void Container_MousePress(UIComponent sender, Events.MouseEventArgs e)
    {
        if (SelectedIndex.Value != null)
        {
            var previouslySelectedPanel = _verticalLayout.Panels.ElementAt((int)SelectedIndex.Value);
            previouslySelectedPanel.BackgroundColor.Value = Color.Transparent;
            previouslySelectedPanel.BackgroundColor.RemoveBinding();
        }

        var selectedPanel = (Panel)sender;
        var index = _verticalLayout.Panels.ToList().IndexOf(selectedPanel);
        if (index < 0) throw new ArgumentException($"Cannot find element in list box: make sure the element has not been deleted");
        SelectedIndex.Value = (uint)index;
        selectedPanel.BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColor);
    }

    private void SelectedIndex_ValueChange(BindableProperty<uint?> sender, ValueChangeEventArgs<uint?> e)
    {
        var numberOfItems = _verticalLayout.Children.Count();
        if (e.CurrentValue != null && e.CurrentValue.Value > numberOfItems)
        {
            throw new InvalidOperationException($"Cannot select ListBox item with index {e.CurrentValue}: the ListBox has {numberOfItems} elements");
        }

        SelectedItem.Value = e.CurrentValue != null ? Children.ElementAt((int)e.CurrentValue) : null;
    }

    private void SelectedItem_ValueChange(BindableProperty<UIComponent?> sender, ValueChangeEventArgs<UIComponent?> e)
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
}
