using System.Text.RegularExpressions;
using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Collections;

// TODO: optimize. There are too many Linq queries. Cache them.

/// <summary>
/// A hierarchical list of items.
/// </summary>
/// <remarks>
/// Although list boxes are typically used to list text in order to allow selecting one or more options,
/// the <see cref="TreeBox"/> class allows to list not only text but any <see cref="UIComponent"/>.
/// </remarks>
public class TreeBox : UIHierarchicalContainer
{
    private static readonly PixelUnit FoldUnfoldButtonSize = 20;
    private static readonly PixelUnit IndentationSize = 20;

    private readonly ListBox _listBox;
    private readonly List<TreeNode> _nodes = [];

    public UIProperty<TreeNode?> SelectedNode { get; }

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

        Items.ValueAdd.Subscribe(index => AddTreeNode((TreeNode)Items[index]));
        Items.ValueRemove.Subscribe(index => InternalRemoveTreeNode((TreeNode)Items[index]));

        SelectedNode = new UIProperty<TreeNode?>(this, (TreeNode?)null);
        // TODO SelectedNode can be bound to _listBox.SelectedItem and converters!
        SelectedNode.ValueChange.Subscribe(OnSelectedNodeChange);
        _listBox.SelectedIndex.ValueChange.Subscribe(OnSelectedIndexChange);
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.First();

    internal void AddTreeNode(TreeNode component, TreeNode? after = null)
    {
        component._root = this;
        var gridLayout = new GridLayout(Application);
        var foldUnfoldButton = new Button(Application);
        foldUnfoldButton.Text.Value = "*";
        foldUnfoldButton.Width.Value = FoldUnfoldButtonSize;
        foldUnfoldButton.Height.Value = FoldUnfoldButtonSize;
        // TODO: Unsubscribe these events.
        foldUnfoldButton.Action += (_, _) =>
        {
            component.Collapsed.Value = !component.Collapsed.Value;
            UpdateCollapseState(component, recursive: true);
        };

        var panel = new Panel(Application);

        gridLayout.Rows.Value = 1;
        gridLayout.Columns.Value = 3u;
        gridLayout.ColumnSizes.Value = $"{IndentationSize * component.Indentation}px,{FoldUnfoldButtonSize}px,*";
        gridLayout.Items.Add(panel);
        gridLayout.Items.Add(foldUnfoldButton);
        gridLayout.Items.Add(component.Component.Value);

        gridLayout.AutoWidth.Value = AutoSize.Wrap;
        gridLayout.AutoHeight.Value = AutoSize.Wrap;
        gridLayout.HorizontalAlignment.Value = Alignment.Left;
        gridLayout.VerticalAlignment.Value = Alignment.Top;

        UIComponent? afterComponent;

        if (after is not null)
        {
            afterComponent = after.Component.Value;
            foreach (var child in after.GetRecursiveChildren())
            {
                if (Children.Any(c => c == child.Component.Value))
                {
                    afterComponent = child.Component.Value;
                }
            }

            var afterGridLayout = _listBox.Items
                                          .Cast<GridLayout>()
                                          .First(c => c.Items[2] == afterComponent);
            var index1 = _listBox.Items.IndexOf(afterGridLayout);
            _listBox.Items.Insert(index1, gridLayout);
            var index = _listBox.Items.Cast<GridLayout>().ToList().FindIndex(c => c == afterGridLayout);
            if (index + 1 < _listBox.Items.Count) _nodes.Insert(index + 1, component);
            else _nodes.Add(component);

            UpdateCollapseState(after);
        }
        else
        {
            _listBox.Items.Add(gridLayout);
            _nodes.Add(component);
        }

        foreach (var child in component.Children)
        {
            AddTreeNode((TreeNode)child, component);
        }

        component.Collapsed.ValueChange.Subscribe(_ => UpdateCollapseState(component));
    }

    internal void InternalRemoveTreeNode(TreeNode node)
    {
        var elementIndex = Children.ToList().FindIndex(c => c == node.Component.Value);
        if (elementIndex < 0) throw new ArgumentException("Element not found in tree box");
        _listBox.Items.RemoveAt(elementIndex);
        _nodes.Remove(node);

        var childrenCopy = new List<TreeNode>(node.Children.Cast<TreeNode>());
        foreach (var child in childrenCopy)
        {
            node.Children.Remove(child);
        }
    }

    public void Clear() =>
        _nodes.Where(n => n.Parent == null).ToList().ForEach(InternalRemoveTreeNode);

    private void OnSelectedNodeChange(TreeNode? node)
    {
        if (node == null)
        {
            _listBox.SelectedIndex.Value = null;
        }
        else
        {
            var selectedIndex = Children.ToList().FindIndex(c => c == node.Component.Value);
            if (selectedIndex < 0) throw new ArgumentException("Selected node not found");
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
            var selectedNode = _nodes[(int)index];
            SelectedNode.Value = selectedNode;
        }
    }

    private void UpdateCollapseState(TreeNode node, bool recursive = false)
    {
        var gridLayout = _listBox.Items.Cast<GridLayout>().ToList().Find(c => c.Items.ElementAt(2) == node.Component.Value) ?? throw new ArgumentException("Cannot find node to expand in tree box");
        var collapseButton = (Button)gridLayout.Items[1];

        // TODO: when using column and row sizes as lists instead of strings no need to use regex
        var pattern = @"(?<=^.*?,)(\d+)px";
        var zeroPixelReplacement = "0px";
        var buttonSizeReplacement = $"{FoldUnfoldButtonSize}px";

        if (node.Children.Count == 0)
        {
            collapseButton.Visibility.Value = ComponentVisibility.Hidden;
            gridLayout.ColumnSizes.Value = Regex.Replace(gridLayout.ColumnSizes.Value, pattern, zeroPixelReplacement);
        }
        else if (node.Collapsed.Value)
        {
            collapseButton.Visibility.Value = ComponentVisibility.Visible;
            collapseButton.Text.Value = "+";
            gridLayout.ColumnSizes.Value = Regex.Replace(gridLayout.ColumnSizes.Value, pattern, buttonSizeReplacement);
        }
        else
        {
            collapseButton.Visibility.Value = ComponentVisibility.Visible;
            collapseButton.Text.Value = "-";
            gridLayout.ColumnSizes.Value = Regex.Replace(gridLayout.ColumnSizes.Value, pattern, buttonSizeReplacement);
        }

        if (node.ShouldDisplay)
        {
            gridLayout.Visibility.Value = ComponentVisibility.Visible;
        }
        else
        {
            gridLayout.Visibility.Value = ComponentVisibility.Collapsed;
        }

        if (recursive)
        {
            foreach (var child in node.Children)
            {
                UpdateCollapseState((TreeNode)child, true);
            }
        }
    }
}
