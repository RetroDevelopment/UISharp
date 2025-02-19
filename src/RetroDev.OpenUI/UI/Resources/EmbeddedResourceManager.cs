namespace RetroDev.OpenUI.UI.Resources;

/// <summary>
/// Manages resources in the application.
/// All resources are treated as embedded resources, so they are assumed to be emebedded in some of the
/// assembly running.
/// </summary>
public class EmbeddedResourceManager : IResourceManager
{
    /// <summary>
    /// Manages windows xml UI definition resources.
    /// </summary>
    public ITextResources Windows { get; } = new EmbeddedWindowResources();

    /// <summary>
    /// Manages themes xml resources.
    /// </summary>
    public ITextResources Themes { get; } = new EmbeddedThemeResources();

    /// <summary>
    /// Manages font ttf resources.
    /// </summary>
    public IBinaryResources Fonts { get; } = new EmbeddedFontResources();

    // public ITextResources Langauges => new LanguageResources();
}
