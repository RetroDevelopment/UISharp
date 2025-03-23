using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Collections;

// TODO: not really thread safe
// TODO: TreeNode is not yet ready to be used in xml UIDefinition.

/// <summary>
/// Displays a node of a <see cref="TreeBox"/>.
/// </summary>
public class TreeNode : UINode
{
    internal TreeBox? _root;
    private int indentation = 0;

    public UIProperty<bool> Collapsed { get; set; }
    public TreeNode? Parent { get; private set; }

    public bool ShouldDisplay
    {
        get
        {
            if (Parent == null)
            {
                return true;
            }

            if (Parent is not null && Parent.ShouldDisplay && !Parent.Collapsed.Value)
            {
                return true;
            }

            return false;
        }
    }

    internal int Indentation => indentation;

    public TreeNode(UIWidget component) : base(component)
    {
        Collapsed = new UIProperty<bool>(component.Application, false);
        Children.ValueAdd.Subscribe(OnChildAdd);
        Children.ValueRemove.Subscribe(OnChildRemove);
    }

    private void OnChildAdd(int index)
    {
        var child = (TreeNode)Children[index];
        child.indentation = indentation + 1;
        _root?.AddTreeNode(child, this);
        child.Parent = this;
        child.Parent.Collapsed.Value = false;
    }

    private void OnChildRemove(int index)
    {
        var child = (TreeNode)Children[index];
        _root?.InternalRemoveTreeNode(child);
    }

    public IEnumerable<TreeNode> GetRecursiveChildren()
    {
        List<TreeNode> children = [];

        foreach (var child in Children.Cast<TreeNode>())
        {
            children.Add(child);
            children.AddRange(child.GetRecursiveChildren());
        }

        return children;
    }
}
