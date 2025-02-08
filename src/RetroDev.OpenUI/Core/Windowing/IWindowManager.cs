using RetroDev.OpenUI.Core.Contexts;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenUI.Core.Windowing;

/// <summary>
/// Creates and manages windows.
public interface IWindowManager
{
    /// <summary>
    /// Gets the size of the main display.
    /// </summary>
    Size ScreenSize { get; }

    /// <summary>
    /// The system that emits event using <see langword="this" /> <see cref="IWindowManager{TWindowId}"/>.
    /// </summary>
    IEventSystem EventSystem { get; }

    /// <summary>
    /// Initializes the UI Environment.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Creates a new window without showing it.
    /// </summary>
    /// <param name="context">
    /// The rendering context for the window. It must be compatible with <see langword="this" /> <see cref="IWindowManager"/>
    /// implementation.
    /// </param>
    /// <returns>The window identifier.</returns>
    /// <exception cref="UIInitializationException">If the given <paramref name="context"/> is not compatible.</exception>
    IWindowId CreateWindow(IRenderingContext context);

    /// <summary>
    /// Shows the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to show.</param>
    void ShowWindow(IWindowId windowId);

    /// <summary>
    /// Hides the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to hide.</param>
    void HideWindow(IWindowId windowId);

    /// <summary>
    /// Sets the area where to render the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to change rendering area.</param>
    /// <param name="renderingArea">The new rendering area.</param>
    void SetRenderingArea(IWindowId windowId, Area renderingArea);

    /// <summary>
    /// Sets the title for the window with the given <paramref name="windowId"/>-
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set the title.</param>
    /// <param name="title">The window title.</param>
    void SetTitle(IWindowId windowId, string title);

    /// <summary>
    /// Closes the UI environment. Make sure that no other SDL operation is performed after calling this method!
    /// </summary>
    public void Shutdown();
}
