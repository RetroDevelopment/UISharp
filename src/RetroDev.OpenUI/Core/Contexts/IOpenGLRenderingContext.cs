namespace RetroDev.OpenUI.Core.Contexts;

/// <summary>
/// A rendering context that uses some window manager targeting OpenGL.
/// </summary>
public interface IOpenGLRenderingContext : IRenderingContext
{
    /// <summary>
    /// Sets the OpenGL state machine to use <see langword="this" /> context.
    /// </summary>
    void MakeCurrent();

    /// <summary>
    /// Renders the current frame into the window.
    /// </summary>
    void RenderFrame();
}
