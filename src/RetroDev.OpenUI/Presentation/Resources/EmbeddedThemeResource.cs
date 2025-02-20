namespace RetroDev.OpenUI.Presentation.Resources;

/// <summary>
/// Allows to locate a theme resource.
/// </summary>
public class EmbeddedThemeResources : EmbeddedResourcesBase, ITextResources
{
    public EmbeddedThemeResources() : base("Assets.Themes") { }

    /// <summary>
    /// Gets the xml text of the resource with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The resource name. If name is "theme", then the embedded resource "theme.xml" will be loaded.</param>
    /// <returns>The xml content of the theme.</returns>
    /// <exception cref="FileNotFoundException">If the resource could not be located.</exception>
    public string this[string name] =>
        LoadEmbeddedStringResource($"{name}.xml");
}
