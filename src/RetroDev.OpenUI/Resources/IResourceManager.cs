namespace RetroDev.OpenUI.Resources;

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
}
