namespace RetroDev.OpenUI.UI.Resources;

/// <summary>
/// Allows to locate a text resource.
/// </summary>
public interface ITextResources
{
    /// <summary>
    /// Gets the text of the resource with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The resource name.</param>
    /// <returns>The text content of the resource.</returns>
    string this[string name] { get; }
}
