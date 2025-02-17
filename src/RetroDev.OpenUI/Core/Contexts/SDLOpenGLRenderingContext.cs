using OpenTK;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Windowing.SDL;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Utils;
using SDL2;
using static SDL2.SDL;

namespace RetroDev.OpenUI.Core.Contexts;

/// <summary>
/// The rendering context created by SDL2 for OpenGL.
/// </summary>
public class SDLOpenGLRenderingContext : ISDLRenderingContext, IOpenGLRenderingContext
{
    private class SDL2OpenGLBindings : IBindingsContext
    {
        public nint GetProcAddress(string procName)
        {
            return SDL.SDL_GL_GetProcAddress(procName);
        }
    }

    private readonly Application _application;
    private readonly nint _openGlContext;

    /// <summary>
    /// The identiier of the window created by <see langword="this" /> <see cref="IRenderingContext"/>.
    /// </summary>
    public SDLWindowId WindowId { get; }

    /// <summary>
    /// Creates a new rendering context.
    /// </summary>
    /// <param name="application">The application that owns this context.</param>
    /// <param name="openGlVersion">The OpenGL verison. If <see langword="null" /> the defulat version will be used.</param>
    /// <exception cref="UIInitializationException">If an error occurs during window creation.</exception>
    internal SDLOpenGLRenderingContext(Application application, Version? openGlVersion = null)
    {
        _application = application;
        var openglActionVersion = openGlVersion ?? new Version(3, 3);
        _application.Logger.LogInfo($"Using OpenGL version {openglActionVersion}");
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, openglActionVersion.Major), _application.Logger);
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, openglActionVersion.Minor), _application.Logger);
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE), _application.Logger);

        // Enable multisampling and set the number of samples
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 1), _application.Logger); // Enable multisampling
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, 16), _application.Logger);  // Number of samples (e.g., 4 for 4x MSAA)

        var handle = SDL_CreateWindow(string.Empty,
                                      SDL_WINDOWPOS_CENTERED,
                                      SDL_WINDOWPOS_CENTERED,
                                      800, 600,
                                      SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL_WindowFlags.SDL_WINDOW_OPENGL | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

        if (handle == nint.Zero) throw new UIInitializationException($"Error creating window: {SDL_GetError()}");
        WindowId = new SDLWindowId(handle);
        _openGlContext = SDL_GL_CreateContext(handle);
        if (_openGlContext == nint.Zero)
        {
            throw new UIInitializationException($"Unable to create OpenGL context: {SDL_GetError()}");
        }

        MakeCurrent();
        // Enable VSync
        LoggingUtils.SDLCheck(() => SDL_GL_SetSwapInterval(1), application.Logger, warning: true);
    }

    /// <summary>
    /// Sets the OpenGL state machine to use the SDL window created by this context.
    /// </summary>
    public void MakeCurrent() =>
        LoggingUtils.SDLCheck(() => SDL_GL_MakeCurrent(WindowId.Handle, _openGlContext), _application.Logger);

    /// <summary>
    /// Render the curent frame into the window.
    /// </summary>
    public void RenderFrame()
    {
        SDL_GL_SwapWindow(WindowId.Handle);
    }

    /// <summary>
    /// Loads OpenGL library using SDL.
    /// </summary>
    public void LoadBinding()
    {
        LoggingUtils.OpenGLCheck(() => GL.LoadBindings(new SDL2OpenGLBindings()), _application.Logger);
    }
}
