using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.Core;

/// <summary>
/// Contains UI environment information and performs generic UI environment operations such as initializing the UI.
/// </summary>
public interface IUIEnvironment
{
    /// <summary>
    /// Initializes the UI Environment.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Closes the UI environment. Make sure that no other SDL operation is performed after calling this method!
    /// </summary>
    public void Shutdown();
}
