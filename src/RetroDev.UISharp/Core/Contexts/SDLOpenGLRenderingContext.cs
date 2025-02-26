using OpenTK;
using OpenTK.Graphics.OpenGL;
using RetroDev.UISharp.Core.Exceptions;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Core.Windowing.SDL;
using static SDL2.SDL;

namespace RetroDev.UISharp.Core.Contexts;

/// <summary>
/// The rendering context created by SDL2 for OpenGL.
/// </summary>
public class SDLOpenGLRenderingContext : ISDLRenderingContext, IOpenGLRenderingContext
{
    private class SDL2OpenGLBindings : IBindingsContext
    {
        public nint GetProcAddress(string procName)
        {
            return SDL_GL_GetProcAddress(procName);
        }
    }

    private readonly ILogger _logger;
    private readonly nint _openGlContext;

    /// <summary>
    /// The identiier of the window created by <see langword="this" /> <see cref="IRenderingContext"/>.
    /// </summary>
    public SDLWindowId WindowId { get; }

    /// <summary>
    /// Creates a new rendering context.
    /// </summary>
    /// <param name="logger">The logger used to log in this class.</param>
    /// <param name="openGlVersion">The OpenGL verison. If <see langword="null" /> the defulat version will be used.</param>
    /// <exception cref="UIInitializationException">If an error occurs during window creation.</exception>
    internal SDLOpenGLRenderingContext(ILogger logger, Version? openGlVersion = null)
    {
        _logger = logger;
        var openglActionVersion = openGlVersion ?? new Version(3, 3);
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, openglActionVersion.Major), _logger);
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, openglActionVersion.Minor), _logger);
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE), _logger);

        // Enable multisampling and set the number of samples
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_MULTISAMPLEBUFFERS, 1), _logger); // Enable multisampling
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_MULTISAMPLESAMPLES, 16), _logger);  // Number of samples (e.g., 4 for 4x MSAA)
        LoggingUtils.SDLCheck(() => SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_DEPTH_SIZE, 24), _logger);

        var handle = SDL_CreateWindow(string.Empty,
                                      SDL_WINDOWPOS_CENTERED,
                                      SDL_WINDOWPOS_CENTERED,
                                      800, 600,
                                      SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL_WindowFlags.SDL_WINDOW_OPENGL);

        if (handle == nint.Zero) throw new UIInitializationException($"Error creating window: {SDL_GetError()}");
        WindowId = new SDLWindowId(handle);
        _openGlContext = SDL_GL_CreateContext(handle);
        if (_openGlContext == nint.Zero)
        {
            throw new UIInitializationException($"Unable to create OpenGL context: {SDL_GetError()}");
        }

        MakeCurrent();
        CheckDepthBufferSize();
        // Enable VSync
        LoggingUtils.SDLCheck(() => SDL_GL_SetSwapInterval(1), _logger, warning: true);
    }

    /// <summary>
    /// Sets the OpenGL state machine to use the SDL window created by this context.
    /// </summary>
    public void MakeCurrent() =>
        LoggingUtils.SDLCheck(() => SDL_GL_MakeCurrent(WindowId.Handle, _openGlContext), _logger);

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
        LoggingUtils.OpenGLCheck(() => GL.LoadBindings(new SDL2OpenGLBindings()), _logger);
    }

    private void CheckDepthBufferSize()
    {
        var sdlDepthSize = 0;
        LoggingUtils.SDLCheck(() => SDL_GL_GetAttribute(SDL_GLattr.SDL_GL_DEPTH_SIZE, out sdlDepthSize), _logger);
        if (sdlDepthSize < 24)
        {
            _logger.LogError($"SDL depth buffer has {sdlDepthSize} bits: it must have at least 24 bits to render correctly!");
        }
    }
}
