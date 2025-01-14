using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Containers;

// TODO: not really thread safe
// TODO: TreeNode is not yet ready to be used in xml UIDefinition.

/// <summary>
/// Displays a node of a <see cref="TreeBox"/>.
/// </summary>
public class TreeNode
{
    internal readonly List<TreeNode> _children = [];
    internal TreeBox? _root;
    private int indentation = 0;

    // TODO: pass LifeCycle to property so that you can treat is a UI
    public BindableProperty<TreeNode, UIComponent> Content { get; set; }
    public BindableProperty<TreeNode, bool> Collapsed { get; set; }
    public TreeNode? Parent { get; private set; }

    public bool ShouldDisplay
    {
        get
        {
            if (Parent == null)
            {
                return true;
            }

            if (Parent != null && Parent.ShouldDisplay && !Parent.Collapsed)
            {
                return true;
            }

            return false;
        }
    }

    internal int Indentation => indentation;

    public TreeNode(UIComponent component)
    {
        Content = new BindableProperty<TreeNode, UIComponent>(this, component);
        Collapsed = new BindableProperty<TreeNode, bool>(this, false);
    }

    public void AddChild(TreeNode child)
    {
        // TODO: other way to check life cycle when not tree node? For example pass the app as constructor
        _root?.Application?.LifeCycle?.ThrowIfPropertyCannotBeSet();
        child.indentation = indentation + 1;
        _root?.AddTreeNode(child, this);
        _children.Add(child);
        child.Parent = this;
        child.Parent.Collapsed.Value = false;
    }

    public void RemoveChild(TreeNode child)
    {
        _root?.Application?.LifeCycle?.ThrowIfPropertyCannotBeSet();

        var index = _children.FindIndex(c => c == child);
        if (index < 0) throw new ArgumentException("Cannod find child node to remove.");
        _root?.InternalRemoveTreeNode(child);
        _children.RemoveAt(index);
    }

    public IEnumerable<TreeNode> GetRecursiveChildren()
    {
        List<TreeNode> children = [];

        foreach (var child in _children)
        {
            children.Add(child);
            children.AddRange(child.GetRecursiveChildren());
        }

        return children;
    }
}
