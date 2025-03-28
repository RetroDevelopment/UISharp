using System.Net.Http.Headers;

namespace RetroDev.UISharp.Presentation.Properties;

public static class UITreePropertyExtensions
{
    /// <summary>
    /// Converts a hierarchical object into a <see cref="UIPropertyHierarchy{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The object value type.</typeparam>
    /// <param name="this">The object to convert.</param>
    /// <param name="application">The running application.</param>
    /// <param name="getChildren">The function that allows selecting the object hierarchical children.</param>
    /// <returns>A <see cref="UITreeNode{TValue}"/> instance mapping <paramref name="this"/> object.</returns>
    public static UIPropertyHierarchy<TValue> ToUIPropertyHierarchy<TValue>(this TValue @this, Application application, Func<TValue, IEnumerable<TValue>> getChildren)
    {
        var hierarchy = new UIPropertyHierarchy<TValue>(application);
        hierarchy.Children.Add(@this.ToUITreeNode(application, getChildren));
        return hierarchy;
    }

    /// <summary>
    /// Converts a hierarchical object into a <see cref="UITreeNode{TValue}"/>.
    /// </summary>
    /// <typeparam name="TValue">The object value type.</typeparam>
    /// <param name="this">The object to convert.</param>
    /// <param name="application">The running application.</param>
    /// <param name="getChildren">The function that allows selecting the object hierarchical children.</param>
    /// <returns>A <see cref="UITreeNode{TValue}"/> instance mapping <paramref name="this"/> object.</returns>
    public static UITreeNode<TValue> ToUITreeNode<TValue>(this TValue @this, Application application, Func<TValue, IEnumerable<TValue>> getChildren)
    {
        var node = new UITreeNode<TValue>(application, @this);
        var children = getChildren(@this);
        foreach (var child in children)
        {
            node.Children.Add(child.ToUITreeNode(application, getChildren));
        }

        return node;
    }
}
