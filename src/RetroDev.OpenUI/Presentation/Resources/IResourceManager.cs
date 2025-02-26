namespace RetroDev.OpenUI.Presentation.Resources;

/// <summary>
/// Manages resources in the application.
/// </summary>
public interface IResourceManager
{
    /// <summary>
    /// Manages windows xml UI definition resources.
    /// </summary>
    ITextResources Windows { get; }

    /// <summary>
    /// Manages theme xml resouces.
    /// </summary>
    ITextResources Themes { get; }

    /// <summary>
    /// Manages resources in the application.
    /// All resources are treated as embedded resources, so they are assumed to be emebedded in some of the
    /// assembly running.
    /// </summary>
    IBinaryResources Fonts { get; }
}
