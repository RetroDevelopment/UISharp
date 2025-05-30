﻿using RetroDev.UISharp.Core.Contexts;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Exceptions;

namespace RetroDev.UISharp.Core.Windowing;

/// <summary>
/// Creates and manages windows.
public interface IWindowManager
{
    /// <summary>
    /// Gets the size of the main display.
    /// </summary>
    Size ScreenSize { get; }

    /// <summary>
    /// The mouse cursor shape to display.
    /// </summary>
    MouseCursor Cursor { set; get; }

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
    void SetWindowRenderingArea(IWindowId windowId, Area renderingArea);

    /// <summary>
    /// Sets the title for the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set the title.</param>
    /// <param name="title">The window title.</param>
    void SetTitle(IWindowId windowId, string title);

    /// <summary>
    /// Sets the opacity of the window with the given <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set opacity.</param>
    /// <param name="opacity">The opacity, where 0 means fully transparent and 255 means fully opaque.</param>
    void SetOpacity(IWindowId windowId, byte opacity);

    /// <summary>
    /// Sets whether the window with the given <paramref name="windowId"/> is reiszable.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set if it is resizable.</param>
    /// <param name="resizable"><see langword="true" /> if the window is resizable, otherwise <see langword="false" />.</param>
    void SetReSizable(IWindowId windowId, bool resizable);

    /// <summary>
    /// Focuses the window with <paramref name="windowId"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to focus.</param>
    void FocusWindow(IWindowId windowId);

    /// <summary>
    /// Maximizes the window with <paramref name="windowID"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to maximize.</param>
    void Maximize(IWindowId windowID);

    /// <summary>
    /// Minimizes the window with <paramref name="windowID"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to minimize.</param>
    void Minimize(IWindowId windowID);

    /// <summary>
    /// Sets the window with <paramref name="windowID"/> in full screen mode.
    /// </summary>
    /// <param name="windowId">The identifier of the window to set in full screen.</param>
    void SetFullScreen(IWindowId windowID);

    /// <summary>
    /// Restores the window with <paramref name="windowID"/> from full screen mode.
    /// </summary>
    /// <param name="windowId">The identifier of the window to restore from full screen.</param>
    public void RestoreFullScreen(IWindowId windowID);

    /// <summary>
    /// Restores maximize and minimize state of the window with <paramref name="windowID"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window to maximize.</param>
    void RestoreWindow(IWindowId windowId);

    /// <summary>
    /// Sets the window minimum size informing the operating system that user cannot in any way resize the window identified by the given
    /// <paramref name="windowId"/> below the given <paramref name="size"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set the minimum size.</param>
    /// <param name="size">The window minimum size.</param>
    void SetWindowMinimumSize(IWindowId windowId, Size size);

    /// <summary>
    /// Sets the window maximum size informing the operating system that user cannot in any way resize the window identified by the given
    /// <paramref name="windowId"/> above the given <paramref name="size"/>.
    /// </summary>
    /// <param name="windowId">The identifier of the window for which to set the maximum size.</param>
    /// <param name="size">The window maximum size.</param>
    void SetWindowMaximumSize(IWindowId windowId, Size size);

    /// <summary>
    /// Copies the given <paramref name="text"/> to the clipboard.
    /// </summary>
    /// <param name="text">The text to copy.</param>
    void CopyToClipboard(string text);

    /// <summary>
    /// Gets the text in the clipboard.
    /// </summary>
    /// <returns>The clipboard text.</returns>
    string GetClipboardContent();

    /// <summary>
    /// Closes the UI environment. Make sure that no other SDL operation is performed after calling this method!
    /// </summary>
    public void Shutdown();
}
