using System.Text.RegularExpressions;
using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Presentation.Properties;

namespace RetroDev.OpenUI.Components.Containers;

// TODO: optimize. There are too many Linq queries. Cache them.
// TODO: maybe create a Mapper class mapping TreeNode into ListBox elements.

/// <summary>
/// A hierarchical list of items.
/// </summary>
/// <remarks>
/// Although list boxes are typically used to list text in order to allow selecting one or more options,
/// the <see cref="TreeBox"/> class allows to list not only text but any <see cref="UIComponent"/>.
/// </remarks>
public class TreeBox : UIContainer
{
    private static readonly PixelUnit FoldUnfoldButtonSize = 20;
    private static readonly PixelUnit IndentationSize = 20;

    private readonly ListBox _listBox;
    private readonly List<TreeNode> _nodes = [];

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.First();

    public override IEnumerable<UIWidget> Children => _listBox.Children.Cast<GridLayout>().Select(c => c.Children.ElementAt(2));

    public UIProperty<TreeBox, TreeNode?> SelectedNode { get; }

    /// <summary>
    /// Creates a new tree box.
    /// </summary>
    /// <param name="parent">The application that contain this scroll view.</param>
    public TreeBox(Application application) : base(application)
    {
        _listBox = new ListBox(application);
        AddChildNode(_listBox);

        _listBox.AutoWidth.Value = AutoSize.Stretch;
        _listBox.AutoHeight.Value = AutoSize.Stretch;

        SelectedNode = new UIProperty<TreeBox, TreeNode?>(this, (TreeNode?)null);
        // TODO SelectedNode can be bound to _listBox.SelectedItem and converters!
        SelectedNode.ValueChange += SelectedNode_ValueChange;
        _listBox.SelectedIndex.ValueChange += SelectedIndex_ValueChange;
    }

    public void AddTreeNode(TreeNode component)
    {
        AddTreeNode(component, null);
    }

    internal void AddTreeNode(TreeNode component, TreeNode? after = null)
    {
        component._root = this;
        var gridLayout = new GridLayout(Application);
        var foldUnfoldButton = new Button(Application);
        foldUnfoldButton.Text.Value = "*";
        foldUnfoldButton.Width.Value = FoldUnfoldButtonSize;
        foldUnfoldButton.Height.Value = FoldUnfoldButtonSize;
        foldUnfoldButton.Action += (_, _) =>
        {
            component.Collapsed.Value = !component.Collapsed.Value;
            UpdateCollapseState(component, recursive: true);
        };

        var panel = new Panel(Application);

        gridLayout.Rows.Value = 1;
        gridLayout.Columns.Value = 3u;
        gridLayout.ColumnSizes.Value = $"{IndentationSize * component.Indentation}px,{FoldUnfoldButtonSize}px,*";
        gridLayout.AddComponent(panel);
        gridLayout.AddComponent(foldUnfoldButton);
        gridLayout.AddComponent(component.Content.Value);

        gridLayout.AutoWidth.Value = AutoSize.Wrap;
        gridLayout.AutoHeight.Value = AutoSize.Wrap;
        gridLayout.HorizontalAlignment.Value = Alignment.Left;
        gridLayout.VerticalAlignment.Value = Alignment.Top;

        UIComponent? afterComponent;

        if (after != null)
        {
            afterComponent = after.Content.Value;
            foreach (var child in after.GetRecursiveChildren())
            {
                if (Children.Any(c => c == child.Content.Value))
                {
                    afterComponent = child.Content.Value;
                }
            }

            var afterGridLayout = _listBox.Children
                                          .Cast<GridLayout>()
                                          .First(c => c.Children.ElementAt(2) == afterComponent);
            _listBox.AddComponent(gridLayout, afterGridLayout);
            var index = _listBox.Children.Cast<GridLayout>().ToList().FindIndex(c => c == afterGridLayout);
            if (index + 1 < _listBox.Children.Count()) _nodes.Insert(index + 1, component);
            else _nodes.Add(component);

            UpdateCollapseState(after);
        }
        else
        {
            _listBox.AddComponent(gridLayout);
            _nodes.Add(component);
        }

        foreach (var child in component._children)
        {
            AddTreeNode(child, component);
        }

        UpdateCollapseState(component);
        component.Collapsed.ValueChange += (_, _) => UpdateCollapseState(component);
    }

    public void RemoveTreeNode(TreeNode node)
    {
        if (node.Parent != null)
        {
            node.Parent.RemoveChild(node);
        }
        else
        {
            InternalRemoveTreeNode(node);
        }
    }

    public void InternalRemoveTreeNode(TreeNode node)
    {
        var elementIndex = Children.ToList().FindIndex(c => c == node.Content.Value);
        if (elementIndex < 0) throw new ArgumentException("Element not found in tree box");
        _listBox.RemoveComponent((uint)elementIndex);
        _nodes.Remove(node);

        var childrenCopy = new List<TreeNode>(node._children);
        foreach (var child in childrenCopy)
        {
            node.RemoveChild(child);
        }
    }

    public void Clear() =>
        _nodes.Where(n => n.Parent == null).ToList().ForEach(RemoveTreeNode);

    private void SelectedNode_ValueChange(BindableProperty<TreeNode?> sender, ValueChangeEventArgs<TreeNode?> e)
    {
        if (e.CurrentValue == null)
        {
            _listBox.SelectedIndex.Value = null;
        }
        else
        {
            var selectedIndex = Children.ToList().FindIndex(c => c == e.CurrentValue.Content.Value);
            if (selectedIndex < 0) throw new ArgumentException("Selected node not found");
            _listBox.SelectedIndex.Value = (uint)selectedIndex;
        }
    }

    private void SelectedIndex_ValueChange(BindableProperty<uint?> sender, ValueChangeEventArgs<uint?> e)
    {
        if (e.CurrentValue == null)
        {
            SelectedNode.Value = null;
        }
        else
        {
            var selectedNode = _nodes[(int)e.CurrentValue.Value];
            SelectedNode.Value = selectedNode;
        }
    }

    private void UpdateCollapseState(TreeNode node, bool recursive = false)
    {
        var gridLayout = _listBox.Children.Cast<GridLayout>().ToList().Find(c => c.Children.ElementAt(2) == node.Content.Value) ?? throw new ArgumentException("Cannot find node to expand in tree box");
        var collapseButton = (Button)gridLayout.Children.ElementAt(1);

        // TODO: when using column and row sizes as lists instead of strings no need to use regex
        string pattern = @"(?<=^.*?,)(\d+)px";
        string zeroPixelReplacement = "0px";
        string buttonSizeReplacement = $"{FoldUnfoldButtonSize}px";

        if (node._children.Count == 0)
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
            node._children.ForEach(c => UpdateCollapseState(c, true));
        }
    }
}
