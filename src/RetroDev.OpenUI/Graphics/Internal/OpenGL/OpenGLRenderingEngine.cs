using System.Numerics;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Internal;
using RetroDev.OpenUI.Events.Internal;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Resources;
using RetroDev.OpenUI.Utils;
using SDL2;
using SixLabors.Fonts.Unicode;
using static RetroDev.OpenUI.Graphics.Internal.OpenGL.Model2D;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

// TODO: intro https://learnopengl.com/code_viewer_gh.php?code=src/1.getting_started/2.2.hello_triangle_indexed/hello_triangle_indexed.cpp
// TODO: https://learnopengl.com/code_viewer_gh.php?code=src/1.getting_started/5.2.transformations_exercise2/transformations_exercise2.cpp
// TODO: implement texture atlas for fonts and as mutch vertex model batching as possible, but for now keep it simple

// TODO: move 2D model procedural generation in a separate class (separation of concern) to be possibly
// reused by a future svg to 2D model converter.

// TODO: possible vertex shader layout
// (x, y) (transform ID) (shadow) (u, v)
// And a uniform mat3[] transform; Then for each vertx, let i be the vertex transform ID attribute, use transform[i] on that vertex.
// This allows setting different scaling/translate for each vertex. It is very powerful and it allows to have different border thickness or rectangle corner radius in one vbo.
// Plus use draw instancing to make multiple vbo instances with different uniforms and 1 draw call per vbo (way less draw calls).
// shadow may indicate the shadow alpha channel on each vertex. Then draw a border figure (e.g. circle border, or rectangle border) and get automatically the shadow interpolating the "shadow" attribute.
// (u, v) is the usual texture uv mapping. It could be calculted automatically by the vertex shader since we have boring 2D figures
// but it is critical for texture atlas mapping for font. Also we might have an array of textures in the shader (if possible?) to split the texture atlas
// in case it is too big to accept the limit size. It can get quite complex in the corner case where the texture atlas for 1 label would be so big
// that it should be split in too many textures. But for big fonts consider smaller texture atlas and scaling up loosing resolution.
// Another crazy idea is to parse ttf files vector graphics and make a 2D model with characters, but that takes a lot of time
// to implement so keep it for later. Maybe just fall back to full image whenever font gets big.
// Remember that font optimization is #1 priority since it is expected UI will have a lot of text.

// TODO: possibly have a 4 triangles square all with a vertex in the center of the square. This allows to define
// shadow value of 0 at the center and 1 everywhere else to automatically interpolate shadow factor. But let's see.

// TODO: consider inserting shape borders also in one VBO. With 2 uniforms: background and border color, you can specify
// both borders and background in a single VBO.

// TODO: for rounded corners rectangles, draw all 4 rounded corners in the center and use 4 different transformation matrices
// to scale and translate these corners appropriately. Draw 2 rectangles for the rectangle with missing corners. Also use 2 additional transforms there.
// With the right scale and translate we can have 1 VBO for ALL rounded corners rectangles.
// TODO: same applis for borders. And it will be useful for matrices too. The overall idea is to reduce draw calls and vbo generation.

// TODO: create a vbo per text and map texture atlas
// TODO: batch rendering, maybe join big text in one single model. Text is expected to be the most performance critical due to high number of draw calls. So optimizing text is more critical than optimizing images.
// TODO: start (gradually) imlpementing svg parser that decomposes svg primitives into a 2D model. Rendering is then super efficient and it can be scaled.

/// <summary>
/// The SDL rendering engine used to render on a given window.
/// </summary>
internal class OpenGLRenderingEngine : IRenderingEngine
{
    private class SDL2OpenGLBindings : IBindingsContext
    {
        public nint GetProcAddress(string procName)
        {
            return SDL.SDL_GL_GetProcAddress(procName);
        }
    }

    private readonly Application _application;
    private readonly nint _window;
    private readonly nint _glContext;
    private readonly ShaderProgram _shader;
    private readonly Dictionary<string, int> _textCache = [];

    private readonly int _defaultTexture;

    private readonly ProceduralModelGenerator _modelGenerator;

    /// <summary>
    /// The víewport size in pixels.
    /// </summary>
    public Size ViewportSize { get; set; }

    public OpenGLRenderingEngine(Application application, nint window)
    {
        _application = application;
        _application.LifeCycle.ThrowIfNotOnUIThread();
        _window = window;

        _application.Logger.LogInfo("Using OpenGL rendering");
        _application.Logger.LogDebug("OpenGL Loading shaders");

        var shaderResources = new EmbeddedShaderResources();

        _glContext = SDL.SDL_GL_CreateContext(window);
        if (_glContext == nint.Zero)
        {
            throw new UIInitializationException($"Unable to create OpenGL context: {SDL.SDL_GetError()}");
        }

        LoggingUtils.SDLCheck(() => SDL.SDL_GL_MakeCurrent(window, _glContext), _application.Logger);
        GL.LoadBindings(new SDL2OpenGLBindings()); // Load OpenGL.NET bindings

        LoggingUtils.SDLCheck(() => SDL.SDL_GL_SetSwapInterval(1), application.Logger, warning: true); // Enable VSync
        _shader = new ShaderProgram([new Shader(ShaderType.VertexShader, shaderResources["default.vert"], _application.Logger),
                                     new Shader(ShaderType.FragmentShader, shaderResources["default.frag"], _application.Logger)],
                                     _application.Logger);

        _application.Logger.LogDebug("OpenGL Shaders loaded");

        // SDLCheck(() => SDL.SDL_GL_SetSwapInterval(0)); // This will run the CPU and GPU at 100%
        // GL.Enable(EnableCap.Multisample); // TODO: should enable anti aliasing

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        ViewportSize = new(800, 600); // TODO: SDLWindowEngine will need to update this property when the window size change

        _defaultTexture = CreateTexture(new RgbaImage([0, 0, 0, 0], 1, 1));
        _modelGenerator = new ProceduralModelGenerator();
    }

    /// <summary>
    /// Creates a texture with the given <paramref name="image"/> and stores it in memory.
    /// </summary>
    /// <param name="image">An RGBA image.</param>
    /// <returns>The store texture unique identifier used when referencing this texture.</returns>
    public int CreateTexture(RgbaImage image)
    {
        _application.LifeCycle.ThrowIfNotOnUIThread(); // TODO: texture creation not on UI rendering
        // TODO: now a new texture is created for each text. we will need to probably use a special model with vertices
        // for each character and map it to texture atlas with uv mapping. Text will be the most performance critical
        // issue since there will be a lot of text and that changes often. Plus implement texture garbage collection for text.
        // At the moment a new texture is created and the old one is kept in memory.
        // But for now, let's keep it simple.

        // Texture is RGBA, so [R1, G1, B1, A1, R2, G2, etc.]
        int textureID;
        GL.GenTextures(1, out textureID);
        GL.BindTexture(TextureTarget.Texture2D, textureID);

        // Set texture parameters
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Upload the image data to the texture
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

        // Generate mipmaps for the texture
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        // Unbind the texture
        GL.BindTexture(TextureTarget.Texture2D, 0);

        // Return the texture ID
        return textureID;
    }

    /// <summary>
    /// Renders a rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle shape attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    public void Render(Rectangle rectangle, Area area, Area? clippingArea)
    {
        RenderShape(rectangle, area, clippingArea, _modelGenerator.Rectangle);
    }

    /// <summary>
    /// Renders a circle.
    /// </summary>
    /// <param name="circle">The circle shape attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    public void Render(Circle circle, Area area, Area? clippingArea)
    {
        RenderShape(circle, area, clippingArea, _modelGenerator.Circle);
    }

    /// <summary>
    /// Renders text.
    /// </summary>
    /// <param name="text">The text attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    public void Render(Text text, Area area, Area? clippingArea)
    {
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();
        if (string.IsNullOrEmpty(text.Value)) return;
        var textKey = $"{text.Value}{text.ForegroundColor}";

        if (!_textCache.TryGetValue(textKey, out var textureId))
        {
            var textureImage = new SixLaborsFontRenderingEngine().ConvertTextToRgbaImage(text.Value, 20, text.ForegroundColor);
            textureId = CreateTexture(textureImage);
            _textCache[textKey] = textureId;
        }

        text.TextureID = textureId;

        _shader.Use();
        _shader.SetTransform([area.GetTransformMatrix(ViewportSize, 0.0f, 0.0f),
                              area.GetTransformMatrix(ViewportSize, 0.0f, null)]);
        _shader.SetProjection(ViewportSize.GetPorjectionMatrix());
        _shader.SetFillColor(text.BackgroundColor.ToOpenGLColor());
        _shader.SetClipArea((clippingArea ?? new Area(Point.Zero, ViewportSize)).ToVector4(ViewportSize));
        _shader.SetOffsetMultiplier(OpenTK.Mathematics.Vector2.Zero);
        _modelGenerator.Rectangle.Render(text.TextureID.Value);
    }

    /// <summary>
    /// This method is invoked when starting the rendering of a frame.
    /// </summary>
    /// <param name="backgroundColor">
    /// The frame background color.
    /// </param>
    public void InitializeFrame(Color backroundColor)
    {
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();
        LoggingUtils.SDLCheck(() => SDL.SDL_GL_MakeCurrent(_window, _glContext), _application.Logger);
        var openGlBackgroundColor = backroundColor.ToOpenGLColor();
        GL.ClearColor(openGlBackgroundColor.X, openGlBackgroundColor.Y, openGlBackgroundColor.Z, openGlBackgroundColor.W);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        _modelGenerator.ResetDrawCallsCount();
    }

    /// <summary>
    /// This method is called when the frame rendering is complete.
    /// </summary>
    public void FinalizeFrame()
    {
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();
        SDL.SDL_GL_SwapWindow(_window);
        _application.Logger.LogError($"OpenGL performed {_modelGenerator.DrawCalls} draw calls in the latest frame");
    }

    /// <summary>
    /// Dispose the rendering engine and deallocates all the graphical resources.
    /// </summary>
    public void Shutdown()
    {
        _application.LifeCycle.ThrowIfNotOnUIThread();
        _shader.Close();
    }

    private void RenderShape(IShape shape, Area area, Area? clippingArea, Model2D openglShape)
    {
        // TODO: use barycentric coordinates
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();
        Validate(shape, area);

        var radiusX = 0.0f;
        var radiusY = 0.0f;

        if (shape is Rectangle rectangle)
        {
            radiusX = rectangle.CornerRadiusX ?? 0.0f;
            radiusY = rectangle.CornerRadiusY ?? 0.0f;
        }

        GenericRender(shape.BackgroundColor, area, clippingArea, shape.Rotation, null, radiusX, radiusY, openglShape, shape.TextureID);
        GenericRender(shape.BorderColor, area, clippingArea, shape.Rotation, shape.BorderThickness, radiusX, radiusY, openglShape, shape.TextureID);
    }

    private void GenericRender(Color color,
                               Area area,
                               Area? clippingArea,
                               float rotation,
                               PixelUnit? borderThickness,
                               float xRadius,
                               float yRadius,
                               Model2D openglShape,
                               int? textureId)
    {
        // TODO: use barycentric coordinates
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();

        if (color.IsTransparent) return;

        _shader.Use();

        _shader.SetTransform([area.GetTransformMatrix(ViewportSize, rotation, 0.0f),
                              area.GetTransformMatrix(ViewportSize, rotation, borderThickness)]);
        _shader.SetProjection(ViewportSize.GetPorjectionMatrix());
        _shader.SetFillColor(color.ToOpenGLColor());
        _shader.SetClipArea((clippingArea ?? new Area(Point.Zero, ViewportSize)).ToVector4(ViewportSize));
        _shader.SetOffsetMultiplier(NormalizeRadius(xRadius, yRadius, area.Size));
        openglShape.Render(textureId ?? _defaultTexture);
    }

    private void Validate(IShape shape, Area area)
    {
        var maximumBorderThickness = Math.Min(area.Size.Width, area.Size.Height);
        if (shape.BorderThickness != null && shape.BorderThickness.Value > maximumBorderThickness)
        {
            throw new ArgumentException($"Border thickness {shape.BorderThickness} pixels exceed maximum allowed size of {maximumBorderThickness}");
        }

        if (area.Size.Width < 0.0f) throw new ArgumentException($"Shape negative width ({area.Size.Width}) not allowed");
        if (area.Size.Height < 0.0f) throw new ArgumentException($"Shape negative height ({area.Size.Height}) not allowed");
        if (shape is Rectangle rectangle)
        {
            if (rectangle.CornerRadiusX != null && rectangle.CornerRadiusX.Value > area.Size.Width / 2.0)
            {
                throw new ArgumentException($"Corner radius {rectangle.CornerRadiusX} is too big for rectangle with size {area.Size}");
            }

            if (rectangle.CornerRadiusY != null && rectangle.CornerRadiusY.Value > area.Size.Height / 2.0)
            {
                throw new ArgumentException($"Corner radius {rectangle.CornerRadiusY} is too big for rectangle with size {area.Size}");
            }
        }
    }

    private OpenTK.Mathematics.Vector2 NormalizeRadius(PixelUnit xRadius, PixelUnit yRadius, Size size) =>
        new OpenTK.Mathematics.Vector2(xRadius / size.Width, yRadius / size.Height);
}
