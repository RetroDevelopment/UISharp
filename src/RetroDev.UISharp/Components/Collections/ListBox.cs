using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Collections;

// TODO: implement select with cursor
// TODO: scroll view configuration
// TODO: implement multiple selection

/// <summary>
/// Provides a vertical list of components and a logic to select components.
/// </summary>
/// <remarks>
/// Although list boxes are typically used to list text in order to allow selecting one or more options,
/// the <see cref="ListBox"/> class allows to list not only text but any <see cref="UIObject"/>.
/// </remarks>
public class ListBox : UIContainer
{
    private readonly VerticalLayout _verticalLayout;
    private readonly ScrollView _scrollView;

    /// <summary>
    /// Raised when an item is selected.
    /// <remarks>
    /// There is no need to use this event to detect when a selection changes, use <see cref="SelectedItem"/> or <see cref="SelectedIndex"/>
    /// <see cref="UIProperty{TValue}.ValueChange"/> instead.
    /// This event is only to detect when the user selects an item INCLUDING the already selected item.
    /// </remarks>
    /// </summary>
    public event TypeSafeEventHandler<ListBox, EventArgs>? ItemSelected;

    /// <summary>
    /// The index of the selected element in the list, or <see langword="null" /> if no element is selected.
    /// </summary>
    public UIProperty<uint?> SelectedIndex { get; }

    /// <summary>
    /// The selected element in the list, or <see langword="null" /> if no element is selected.
    /// </summary>
    public UIProperty<UIControl?> SelectedItem { get; }

    /// <summary>
    /// The auto width strategy for the <see langword="this" /> <see cref="ListBox"/> items.
    /// </summary>
    public UIProperty<IAutoSize> ItemsAutoWidth { get; }

    /// <summary>
    /// Creates a new grid layout.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public ListBox(Application application) : base(application)
    {
        _verticalLayout = new VerticalLayout(application);
        _scrollView = new ScrollView(application);

        SelectedIndex = new UIProperty<uint?>(this, (uint?)null);
        SelectedItem = new UIProperty<UIControl?>(this, (UIControl?)null);
        ItemsAutoWidth = new UIProperty<IAutoSize>(this, AutoSize.MaxWrapStretch);
        SelectedIndex.ValueChange.Subscribe(OnSelectedIndexChange);
        SelectedItem.ValueChange.Subscribe(OnSelectedItemChange);

        BackgroundColor.BindTheme(UISharpColorNames.ListBackground);
        BorderColor.BindTheme(UISharpColorNames.ListBorder);

        _scrollView.Item.Value = _verticalLayout;
        _scrollView.AutoWidth.Value = AutoSize.Stretch;
        _scrollView.AutoHeight.Value = AutoSize.Stretch;
        _scrollView.BackgroundColor.Unbind();
        _scrollView.BorderColor.Unbind();
        _scrollView.BackgroundColor.Value = Color.Transparent;
        _scrollView.BorderColor.Value = Color.Transparent;

        _verticalLayout.AutoWidth.BindSourceToDestination(ItemsAutoWidth);
        _verticalLayout.HorizontalAlignment.Value = Alignment.Left;
        _verticalLayout.VerticalAlignment.Value = Alignment.Top;
        _verticalLayout.Margin.BindSourceToDestination(Padding);

        _verticalLayout.Items.BindSourceToDestination(Items);
        _verticalLayout.Items.ValueAdd.Subscribe(index =>
        {
            var container = _verticalLayout.Cells[index];
            container.MousePress += Container_MousePress;
            container.MouseEnter += Container_MouseEnter;
            container.MouseLeave += Container_MouseLeave;
        });
        _verticalLayout.Items.ValueRemove.Subscribe(_ => SelectedIndex.Value = null);

        Children.Add(_scrollView);
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.FirstOrDefault() ?? Size.Zero;

    private void Container_MousePress(UIObject sender, MouseEventArgs e)
    {
        var selectedPanel = (Panel)sender;
        var index = _verticalLayout.Cells.IndexOf(selectedPanel);
        if (index < 0) throw new ArgumentException($"Cannot find element in list box: make sure the element has not been deleted");
        SelectedIndex.Value = (uint)index;
        ItemSelected?.Invoke(this, EventArgs.Empty);
    }

    private void Container_MouseLeave(UIObject sender, EventArgs e)
    {
        if (GetSelectedCell() != sender)
        {
            var panel = (Panel)sender;
            panel.BackgroundColor.Unbind();
            panel.BackgroundColor.Value = Color.Transparent;
        }
    }

    private void Container_MouseEnter(UIObject sender, EventArgs e)
    {
        if (GetSelectedCell() != sender)
        {
            var panel = (Panel)sender;
            panel.BackgroundColor.BindTheme(UISharpColorNames.ListHover);
        }
    }

    private void OnSelectedIndexChange(uint? index)
    {
        var numberOfItems = Items.Count;
        if (index is not null && index > numberOfItems)
        {
            throw new InvalidOperationException($"Cannot select ListBox item with index {index}: the ListBox has {numberOfItems} elements");
        }

        SelectedItem.Value = index is not null ? Items[(int)index] : null;
    }

    private void OnSelectedItemChange(UIControl? item)
    {
        if (item is null)
        {
            SelectedIndex.Value = null;
            return;
        }

        var selectedIndex = Items.IndexOf(item);
        if (selectedIndex == -1) throw new InvalidOperationException("ListBox selected element not found");
        SelectedIndex.Value = (uint)selectedIndex;
        UpdateSelectionGraphics();
    }

    private void UpdateSelectionGraphics()
    {
        if (SelectedItem.PreviousValue is not null)
        {
            var previouslySelectedCell = SelectedItem.PreviousValue.Parent;
            previouslySelectedCell!.BackgroundColor.Value = Color.Transparent;
            previouslySelectedCell!.BackgroundColor.Unbind();
        }

        if (SelectedItem.Value is not null)
        {
            var selectedPanel = SelectedItem.Value.Parent;
            selectedPanel!.BackgroundColor.BindTheme(UISharpColorNames.ListSelection);
        }
    }

    private Panel? GetSelectedCell() =>
        SelectedItem?.Value?.Parent as Panel;
}
