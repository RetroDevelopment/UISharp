namespace RetroDev.OpenUI.UI.Resources;

/// <summary>
/// Allows to locate a text resource.
/// </summary>
public interface IBinaryResources
{
    /// <summary>
    /// Gets the binary content of the resource with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <returns>The binary content of the resource.</returns>
    byte[] this[string name] { get; }
}
