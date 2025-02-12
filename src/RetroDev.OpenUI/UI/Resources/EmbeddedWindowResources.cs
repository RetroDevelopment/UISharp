namespace RetroDev.OpenUI.UI.Resources;

/// <summary>
/// Allows to locate a window resource.
/// </summary>
public class EmbeddedWindowResources : EmbeddedResourcesBase, ITextResources
{
    public EmbeddedWindowResources() : base("Windows") { }

    /// <summary>
    /// Gets the xml text of the resource with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The resource name. If name is "window", then the embedded resource "window.xml" will be loaded.</param>
    /// <returns>The xml content of the UI definition containing the window with the given <paramref name="name"/>.</returns>
    /// <exception cref="FileNotFoundException">If the resource could not be located.</exception>
    public string this[string name] =>
        LoadEmbeddedStringResource($"{name}.xml");
}
