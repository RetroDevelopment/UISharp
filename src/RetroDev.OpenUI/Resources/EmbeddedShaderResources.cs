namespace RetroDev.OpenUI.Resources;

internal class EmbeddedShaderResources : EmbeddedResourcesBase, ITextResources
{
    internal EmbeddedShaderResources() : base("Shaders") { }

    public string this[string name] => LoadEmbeddedStringResource(name);
}
