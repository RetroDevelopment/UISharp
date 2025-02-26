namespace RetroDev.UISharp.Core.Contexts;

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

    /// <summary>
    /// Loads OpenGL library.
    /// This depends on the specific windows management library used by <see langword="this" /> context.
    /// </summary>
    void LoadBinding();
}
