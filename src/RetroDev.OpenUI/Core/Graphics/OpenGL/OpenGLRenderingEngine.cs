using OpenTK;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Contexts;
using RetroDev.OpenUI.Core.Graphics.Fonts;
using RetroDev.OpenUI.Core.Graphics.Shapes;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Resources;
using RetroDev.OpenUI.Utils;
using SDL2;

namespace RetroDev.OpenUI.Core.Graphics.OpenGL;

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
/// The OpenGL rendering engine used to render on a given window.
/// </summary>
public class OpenGLRenderingEngine : IRenderingEngine
{
    private class SDL2OpenGLBindings : IBindingsContext
    {
        public nint GetProcAddress(string procName)
        {
            return SDL.SDL_GL_GetProcAddress(procName);
        }
    }

    private readonly Application _application;
    private readonly IFontRenderingEngine _fontEngine;
    private readonly IOpenGLRenderingContext _context;
    private readonly ShaderProgram _shader;
    private readonly Dictionary<string, int> _textCache = [];

    private readonly int _defaultTexture;

    private readonly ProceduralModelGenerator _modelGenerator;
    private Size _viewportSize = Size.Zero;

    /// <summary>
    /// The víewport size in pixels.
    /// </summary>
    public Size ViewportSize
    {
        get
        {
            _application.LifeCycle.ThrowIfNotOnUIThread();
            return _viewportSize;
        }
        set
        {
            _application.LifeCycle.ThrowIfNotOnUIThread();
            _viewportSize = value;
            _context.MakeCurrent();
            LoggingUtils.OpenGLCheck(() => GL.Viewport(0, 0, (int)value.Width, (int)value.Height), _application.Logger);
        }
    }

    /// <summary>
    /// The rendering context used by this engine to abstract away from window.
    /// </summary>
    public IRenderingContext RenderingContext => _context;

    public OpenGLRenderingEngine(Application application, IOpenGLRenderingContext context, IFontRenderingEngine? fontEngine = null)
    {
        _application = application;
        _application.LifeCycle.ThrowIfNotOnUIThread();
        _context = context;
        _fontEngine = fontEngine ?? new FreeTypeFontRenderingEngine();

        _application.Logger.LogInfo("Using OpenGL rendering");
        _application.Logger.LogDebug("OpenGL Loading shaders");

        var shaderResources = new EmbeddedShaderResources();

        // Load OpenGL.NET bindings
        LoggingUtils.OpenGLCheck(() => GL.LoadBindings(new SDL2OpenGLBindings()), _application.Logger);

        _shader = new ShaderProgram([new Shader(ShaderType.VertexShader, shaderResources["default.vert"], _application.Logger),
                                     new Shader(ShaderType.FragmentShader, shaderResources["default.frag"], _application.Logger)],
                                     _application.Logger);

        _application.Logger.LogDebug("OpenGL Shaders loaded");

        // SDLCheck(() => SDL.SDL_GL_SetSwapInterval(0)); // This will run the CPU and GPU at 100%
        // GL.Enable(EnableCap.Multisample); // TODO: should enable anti aliasing
        LoggingUtils.OpenGLCheck(() => GL.Enable(EnableCap.Blend), _application.Logger);
        LoggingUtils.OpenGLCheck(() => GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha), _application.Logger);

        _defaultTexture = CreateTexture(new RgbaImage([0, 0, 0, 0], 1, 1), interpolate: false);
        _modelGenerator = new ProceduralModelGenerator();
    }

    /// <summary>
    /// Creates a texture with the given <paramref name="image"/> and stores it in memory.
    /// </summary>
    /// <param name="image">An RGBA image.</param>
    /// <param name="interpolate">Whether to interpolate the image or render it as is.</param>
    /// <returns>The store texture unique identifier used when referencing this texture.</returns>
    public int CreateTexture(RgbaImage image, bool interpolate)
    {
        _application.LifeCycle.ThrowIfNotOnUIThread(); // TODO: texture creation not on UI rendering
        var expectedTextureDataSize = image.Width * image.Height * 4;
        if (image.Data.Length != expectedTextureDataSize)
        {
            throw new InvalidOperationException($"Invalid RGBA image size {image.Data.Length}: expected {expectedTextureDataSize} bytes for a {image.Width} x {image.Height} pixels image");
        }

        // TODO: now a new texture is created for each text. we will need to probably use a special model with vertices
        // for each character and map it to texture atlas with uv mapping. Text will be the most performance critical
        // issue since there will be a lot of text and that changes often. Plus implement texture garbage collection for text.
        // At the moment a new texture is created and the old one is kept in memory.
        // But for now, let's keep it simple.

        // Texture is RGBA, so [R1, G1, B1, A1, R2, G2, etc.]
        var textureID = 0;
        LoggingUtils.OpenGLCheck(() => GL.GenTextures(1, out textureID), _application.Logger);
        LoggingUtils.OpenGLCheck(() => GL.BindTexture(TextureTarget.Texture2D, textureID), _application.Logger);

        // Set texture parameters
        var filterType = interpolate ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest;
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat), _application.Logger);
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat), _application.Logger);
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filterType), _application.Logger);
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filterType), _application.Logger);

        // Upload the image data to the texture
        LoggingUtils.OpenGLCheck(() => GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data), _application.Logger);

        // Generate mipmaps for the texture
        LoggingUtils.OpenGLCheck(() => GL.GenerateMipmap(GenerateMipmapTarget.Texture2D), _application.Logger);

        // Unbind the texture
        LoggingUtils.OpenGLCheck(() => GL.BindTexture(TextureTarget.Texture2D, 0), _application.Logger);

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
            var textureImage = _fontEngine.ConvertTextToRgbaImage(text.Value, text.Font, text.ForegroundColor);
            textureId = CreateTexture(textureImage, interpolate: true);
            _textCache[textKey] = textureId;
        }

        text.TextureID = textureId;

        _shader.Use();
        _shader.SetTransform([area.GetTransformMatrix(ViewportSize, 0.0f, 0.0f),
                              area.GetTransformMatrix(ViewportSize, 0.0f, null)]);
        _shader.SetProjection(ViewportSize.GetPorjectionMatrix());
        _shader.SetFillColor(Color.Transparent.ToOpenGLColor());
        _shader.SetClipArea((clippingArea ?? new Area(Point.Zero, ViewportSize)).ToVector4(ViewportSize));
        _shader.SetOffsetMultiplier(OpenTK.Mathematics.Vector2.Zero);
        _shader.SetHasTexture(true);
        _modelGenerator.Rectangle.Render(text.TextureID.Value);
    }

    /// <summary>
    /// Calculates the size to display the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="font">The text font.</param>
    /// <returns>The size to correctly and fully display the given <paramref name="text"/>.</returns>
    public Size ComputeTextSize(string text, Font font) =>
        _fontEngine.ComputeTextSize(text, font);

    /// <summary>
    /// Gets the maximum height occupied by a line of text using the given <paramref name="font"/>.
    /// </summary>
    /// <param name="font">The font for which to compute the height.</param>
    /// <returns>The minimum height necessary to render any character using the given <paramref name="font"/>.</returns>
    public PixelUnit ComputeTextMaximumHeight(Font font) =>
        _fontEngine.ComputeTextMaximumHeight(font);

    /// <summary>
    /// This method is invoked when starting the rendering of a frame.
    /// </summary>
    /// <param name="backgroundColor">
    /// The frame background color.
    /// </param>
    /// <param name="clipArea">
    /// The area within the viewport where to draw. This is used for retained mode rendering: when a component
    /// is invalidated, only its invalidated <see cref="Area"/> needs to be redrawn, so <see cref="InitializeFrame(Color, Area)"/> will
    /// will be called for each invalidated component but only for the given <paramref name="clipArea"/>.
    /// </param>
    public void InitializeFrame(Color backroundColor)
    {
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();
        _context.MakeCurrent();
        var openGlBackgroundColor = backroundColor.ToOpenGLColor();
        LoggingUtils.OpenGLCheck(() => GL.ClearColor(openGlBackgroundColor.X, openGlBackgroundColor.Y, openGlBackgroundColor.Z, openGlBackgroundColor.W), _application.Logger);
        LoggingUtils.OpenGLCheck(() => GL.Clear(ClearBufferMask.ColorBufferBit), _application.Logger);
        _modelGenerator.ResetDrawCallsCount();
    }

    /// <summary>
    /// This method is called when the frame rendering is complete.
    /// </summary>
    public void FinalizeFrame()
    {
        _application.LifeCycle.ThrowIfNotOnRenderingPhase();
        _context.RenderFrame();
        _application.Logger.LogVerbose($"OpenGL performed {_modelGenerator.DrawCalls} draw calls in the latest frame");
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
        _shader.SetHasTexture(textureId != null);
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
