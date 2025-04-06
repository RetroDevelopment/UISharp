using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Collections;

/// <summary>
/// A drop-down control containing a list of items where one can be selected.
/// </summary>
public class DropDown : UIContainer
{
    private readonly PixelUnit ArrowSize = 20.0f;
    private readonly GridLayout _dropdownContent;
    private readonly Panel _selectedItemPanel;

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
    /// Creates a new drop-down item.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public DropDown(Application application) : base(application, autoHeight: AutoSize.Wrap)
    {
        SelectedIndex = new UIProperty<uint?>(this, (uint?)null);
        SelectedItem = new UIProperty<UIControl?>(this, (UIControl?)null);
        ItemsAutoWidth = new UIProperty<IAutoSize>(this, AutoSize.MaxWrapStretch);

        _selectedItemPanel = new Panel(application);

        _dropdownContent = new GridLayout(application, 1, 2);
        _dropdownContent.ColumnSizes.Add(new GridAutoSize());
        _dropdownContent.ColumnSizes.Add(new GridAbsoluteSize(ArrowSize));
        _dropdownContent.Items.Add(_selectedItemPanel);
        _dropdownContent.Items.Add(new Label(application, "▼"));

        BackgroundColor.BindTheme(UISharpColorNames.ListBackground);
        BorderColor.BindTheme(UISharpColorNames.ListBorder);

        SelectedIndex.ValueChange.Subscribe(OnSelectedIndexChange);
        SelectedItem.ValueChange.Subscribe(OnSelectedItemChange);

        Children.Add(_dropdownContent);
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.First();

    private void OnSelectedIndexChange(uint? index)
    {
        var numberOfItems = Items.Count;
        if (index is not null && index > numberOfItems)
        {
            throw new InvalidOperationException($"Cannot select DropDown item with index {index}: the DropDown has {numberOfItems} elements");
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
        if (selectedIndex == -1) throw new InvalidOperationException("DropDown selected element not found");
        SelectedIndex.Value = (uint)selectedIndex;
        _selectedItemPanel.Item.Value = item;
    }
}
