namespace RetroDev.OpenUI.Presentation.Resources;

/// <summary>
/// Allows to locate a font resource.
/// </summary>
public class EmbeddedFontResources : EmbeddedResourcesBase, IBinaryResources
{
    public EmbeddedFontResources() : base("Assets.Fonts") { }

    /// <summary>
    /// Gets the binary representation of the font file with the given <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The resource name. If name is "font" then the embedded resource "font.ttf" will be loaded.</param>
    /// <returns>The binary content of the font file.</returns>
    /// <exception cref="FileNotFoundException">If the resource could not be located.</exception>
    public byte[] this[string name]
    {
        get => LoadEmbeddedBinaryResource($"{name}.ttf");
    }
}
