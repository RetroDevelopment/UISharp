using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Collections;

/// <summary>
/// A hierarchical list of items.
/// </summary>
/// <remarks>
/// Although list boxes are typically used to list text in order to allow selecting one or more options,
/// the <see cref="TreeBox"/> class allows to list not only text but any <see cref="UIObject"/>.
/// </remarks>
public class TreeBox : UIHierarchicalContainer
{
    private static readonly PixelUnit FoldUnfoldButtonSize = 20;
    private static readonly PixelUnit IndentationSize = 20;

    private readonly ListBox _listBox;
    private readonly UIHierarchyFlattenBinder<UIControl, UIControl> _flattenBinder;

    /// <summary>
    /// The selected node or <see langword="null" /> if no node is selected.
    /// </summary>
    public UIProperty<UITreeNode<UIControl>?> SelectedNode { get; }

    /// <summary>
    /// Creates a new tree box.
    /// </summary>
    /// <param name="parent">The application that contain this scroll view.</param>
    public TreeBox(Application application) : base(application)
    {
        BackgroundColor.BindTheme(UISharpColorNames.ListBackground);
        BorderColor.BindTheme(UISharpColorNames.ListBorder);

        _listBox = new ListBox(application);
        _listBox.AutoWidth.Value = AutoSize.Stretch;
        _listBox.AutoHeight.Value = AutoSize.Stretch;
        _listBox.BackgroundColor.Unbind();
        _listBox.BorderColor.Unbind();
        _listBox.BackgroundColor.Value = Color.Transparent;
        _listBox.BorderColor.Value = Color.Transparent;
        _listBox.Margin.BindSourceToDestination(Padding);
        Children.Add(_listBox);

        _flattenBinder = _listBox.Items.FlatBindSourceToDestination(Items, ConvertNodeToListBoxItem);

        SelectedNode = new UIProperty<UITreeNode<UIControl>?>(this, (UITreeNode<UIControl>?)null);

        // TODO SelectedNode can be bound to _listBox.SelectedItem and converters!
        SelectedNode.ValueChange.Subscribe(OnSelectedNodeChange);
        _listBox.SelectedIndex.ValueChange.Subscribe(OnSelectedIndexChange);
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.First();

    private void OnSelectedNodeChange(UITreeNode<UIControl>? node)
    {
        if (node == null)
        {
            _listBox.SelectedIndex.Value = null;
        }
        else
        {
            var selectedItem = _flattenBinder.MapNodeToFlatList(node);
            if (selectedItem is null) throw new ArgumentException($"Selected node {node} not found in this {nameof(TreeBox)} UI component mapping");
            var selectedIndex = _listBox.Items.IndexOf(selectedItem);
            if (selectedIndex < 0) throw new ArgumentException($"Selected node {node} not found in this {nameof(TreeBox)}");
            _listBox.SelectedIndex.Value = (uint)selectedIndex;
        }
    }

    private void OnSelectedIndexChange(uint? index)
    {
        if (index == null)
        {
            SelectedNode.Value = null;
        }
        else
        {
            SelectedNode.Value = _flattenBinder.MapFlatListIndexToNode((int)index);
        }
    }

    private UIControl ConvertNodeToListBoxItem(UITreeNode<UIControl> node)
    {
        var (entry, foldUnfoldButton) = CreateEntry(node);

        foldUnfoldButton.Text.BindSourceToDestination(node.Collapsed, collapsed => collapsed ? "+" : "-");
        foldUnfoldButton.Action += (_, _) => node.Collapsed.Value = !node.Collapsed.Value;

        node.Collapsed.ValueChange.Subscribe(_ => UpdateEntryVisibility(entry, node, recursive: true));
        node.Children.ValueAdd.Subscribe(_ => UpdateEntry(entry, foldUnfoldButton, node));
        node.Children.ValueRemove.Subscribe(_ => UpdateEntry(entry, foldUnfoldButton, node));

        UpdateEntry(entry, foldUnfoldButton, node);

        return entry;
    }

    private (GridLayout entry, Button foldUnfoldButton) CreateEntry(UITreeNode<UIControl> node)
    {
        var entry = new GridLayout(Application, 1, 3);
        var indentation = new Panel(Application);
        var foldUnfoldButton = new Button(Application, string.Empty);
        var content = node.Content.Value;

        foldUnfoldButton.AutoHeight.Value = AutoSize.Wrap;

        entry.Items.AddRange([indentation, foldUnfoldButton, content]);
        entry.ColumnSizes.Clear();
        entry.ColumnSizes.Add(new GridAbsoluteSize(IndentationSize * node.TreeLevel));
        entry.ColumnSizes.Add(new GridAbsoluteSize(FoldUnfoldButtonSize));
        entry.ColumnSizes.Add(new GridAutoSize());
        entry.HorizontalAlignment.Value = Alignment.Left;

        return (entry, foldUnfoldButton);
    }

    private void UpdateEntry(UIControl entry, Button foldUnfoldButton, UITreeNode<UIControl> node)
    {
        UpdateEntryVisibility(entry, node);
        foldUnfoldButton.Visibility.Value = node.Children.Any() ? ComponentVisibility.Visible : ComponentVisibility.Collapsed;
    }

    private void UpdateEntryVisibility(UIControl entry, UITreeNode<UIControl> node, bool recursive = false)
    {
        entry.Visibility.Value = node.AncestorCollapsed ? ComponentVisibility.Collapsed : ComponentVisibility.Visible;
        if (!recursive) return;

        foreach (var childNode in node.Children)
        {
            var childEntry = _flattenBinder.MapNodeToFlatList(childNode);
            if (childEntry is null) continue;
            UpdateEntryVisibility(childEntry, childNode, true);
        }
    }
}
