namespace RetroDev.OpenUI.Resources;

internal class EmbeddedShaderResources : EmbeddedResourcesBase, ITextResources
{
    internal EmbeddedShaderResources() : base("Assets.Shaders") { }

    public string this[string name] => LoadEmbeddedStringResource(name);
}
