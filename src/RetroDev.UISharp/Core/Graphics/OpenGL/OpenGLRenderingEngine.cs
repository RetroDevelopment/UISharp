using System.Collections.Immutable;
using OpenTK.Graphics.OpenGL;
using RetroDev.UISharp.Core.Contexts;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics.Fonts;
using RetroDev.UISharp.Core.Graphics.Imaging;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Core.Windowing;
using RetroDev.UISharp.Presentation.Resources;
using static SDL2.SDL;

namespace RetroDev.UISharp.Core.Graphics.OpenGL;

// TODO: intro https://learnopengl.com/code_viewer_gh.php?code=src/1.getting_started/2.2.hello_triangle_indexed/hello_triangle_indexed.cpp
// TODO: https://learnopengl.com/code_viewer_gh.php?code=src/1.getting_started/5.2.transformations_exercise2/transformations_exercise2.cpp

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

/// <summary>
/// The OpenGL rendering engine used to render on a given window.
/// </summary>
public class OpenGLRenderingEngine : IRenderingEngine
{
    private readonly ThreadDispatcher _dispatcher;
    private readonly ILogger _logger;
    private readonly IFontRenderingEngine _fontEngine;
    private readonly IOpenGLRenderingContext _context;
    private readonly ShaderProgram _shader;
    private readonly Dictionary<string, int> _textCache = [];

    private readonly int _defaultTexture;

    private readonly ProceduralModelGenerator _modelGenerator;

    // TODO: use instanced rendering to batch all recatngles, circles, etc. in 3 draw call (+/- groups for e.g. list boxes which use sparate instances)
    private readonly HashSet<Rectangle> _recangles = [];
    private readonly HashSet<Circle> _circles = [];
    private readonly HashSet<Text> _texts = [];
    // Semi-transparent elements must be rendered in back to front order. Depth buffer would prevent transparent objects to overlap.
    private readonly HashSet<RenderingElement> _semiTransparentElements = [];
    private List<RenderingElement> _backToFrontSortedSemiTransparentElements = [];

    private bool _transparencyChanged = true; // Used to know when to move lists from instance rendering list to _semiTransparentElements

    private Size _viewportSize = Size.Zero;

    /// <summary>
    /// The víewport size in pixels.
    /// </summary>
    public Size ViewportSize
    {
        get
        {
            _dispatcher.ThrowIfNotOnUIThread();
            return _viewportSize;
        }
        set
        {
            _dispatcher.ThrowIfNotOnUIThread();
            _viewportSize = value;
            _context.MakeCurrent();
            LoggingUtils.OpenGLCheck(() => GL.Viewport(0, 0, (int)value.Width, (int)value.Height), _logger);
        }
    }

    /// <summary>
    /// The rendering context used by this engine to abstract away from window.
    /// </summary>
    public IRenderingContext RenderingContext => _context;

    public OpenGLRenderingEngine(ThreadDispatcher dispatcher, ILogger logger, IOpenGLRenderingContext context, IFontRenderingEngine? fontEngine = null)
    {
        dispatcher.ThrowIfNotOnUIThread();

        _dispatcher = dispatcher;
        _logger = logger;
        _context = context;
        _fontEngine = fontEngine ?? new FreeTypeFontRenderingEngine();

        _logger.LogInfo("Using OpenGL rendering");
        _logger.LogDebug("OpenGL Loading shaders");

        // Usually core components like rendering engine should not depend on resources, which are high level components using core. However, it is convenient here
        // in order to locate the shaders from assets.
        var shaderResources = new EmbeddedShaderResources();

        // Load OpenGL.NET bindings
        context.LoadBinding();

        var version = LoggingUtils.OpenGLCheck(() => GL.GetString(StringName.Version), _logger);
        _logger.LogInfo($"OpenGL Version: {version}");

        _shader = new ShaderProgram([new Shader(ShaderType.VertexShader, shaderResources["default.vert"], _logger),
                                     new Shader(ShaderType.FragmentShader, shaderResources["default.frag"], _logger)],
                                     _logger);

        _logger.LogDebug("OpenGL Shaders loaded");

        // SDLCheck(() => SDL.SDL_GL_SetSwapInterval(0)); // This will run the CPU and GPU at 100%
        // GL.Enable(EnableCap.Multisample); // TODO: should enable anti aliasing
        LoggingUtils.OpenGLCheck(() => GL.Enable(EnableCap.Blend), _logger);
        LoggingUtils.OpenGLCheck(() => GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha), _logger);
        LoggingUtils.OpenGLCheck(() => GL.Enable(EnableCap.DepthTest), _logger);
        LoggingUtils.OpenGLCheck(() => GL.DepthFunc(DepthFunction.Less), _logger);

        _defaultTexture = CreateTexture(new RgbaImage([0, 0, 0, 0], new Size(1, 1)), interpolate: false);
        _modelGenerator = new ProceduralModelGenerator();
    }

    /// <summary>
    /// Creates a texture with the given <paramref name="image"/> and stores it in memory.
    /// </summary>
    /// <param name="image">An RGBA image.</param>
    /// <param name="interpolate">Whether to interpolate the image or render it as is.</param>
    /// <returns>The store texture unique identifier used when referencing this texture.</returns>
    public int CreateTexture(Image image, bool interpolate)
    {
        _dispatcher.ThrowIfNotOnUIThread();

        // TODO: now a new texture is created for each text. we will need to probably use a special model with vertices
        // for each character and map it to texture atlas with uv mapping. Text will be the most performance critical
        // issue since there will be a lot of text and that changes often. Plus implement texture garbage collection for text.
        // At the moment a new texture is created and the old one is kept in memory.
        // But for now, let's keep it simple.

        // Texture is RGBA, so [R1, G1, B1, A1, R2, G2, etc.]
        var textureID = 0;
        LoggingUtils.OpenGLCheck(() => GL.GenTextures(1, out textureID), _logger);
        LoggingUtils.OpenGLCheck(() => GL.BindTexture(TextureTarget.Texture2D, textureID), _logger);

        // Set texture parameters
        var filterType = interpolate ? (int)TextureMinFilter.Linear : (int)TextureMinFilter.Nearest;
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge), _logger);
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge), _logger);
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filterType), _logger);
        LoggingUtils.OpenGLCheck(() => GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filterType), _logger);

        // Ensure tight packing.
        LoggingUtils.OpenGLCheck(() => GL.PixelStore(PixelStoreParameter.UnpackAlignment, (int)image.BytesPerPixel), _logger);
        // Upload the image data to the texture
        LoggingUtils.OpenGLCheck(() => GL.TexImage2D(TextureTarget.Texture2D, 0, image.InternalFormat, (int)image.Size.Width, (int)image.Size.Height, 0, image.PixelFormat, image.PixelType, image.Data), _logger);

        // Generate mipmaps for the texture
        LoggingUtils.OpenGLCheck(() => GL.GenerateMipmap(GenerateMipmapTarget.Texture2D), _logger);

        // Unbind the texture
        LoggingUtils.OpenGLCheck(() => GL.BindTexture(TextureTarget.Texture2D, 0), _logger);

        // Return the texture ID
        return textureID;
    }

    /// <summary>
    /// Adds the given <paramref name="rectangle"/> to the rendering canvas.
    /// </summary>
    /// <param name="rectangle">The rectangle to add.</param>
    public void Add(Rectangle rectangle)
    {
        rectangle.ShapeChanged += RenderingElement_ShapeChanged;
        RenderingElement_ShapeChanged(rectangle, EventArgs.Empty);
    }


    /// <summary>
    /// Adds the given <paramref name="circle"/> to the rendering canvas.
    /// </summary>
    /// <param name="circle">The circle to add.</param>
    public void Add(Circle circle)
    {
        circle.ShapeChanged += RenderingElement_ShapeChanged;
        RenderingElement_ShapeChanged(circle, EventArgs.Empty);
    }

    /// <summary>
    /// Adds the given <paramref name="text"/> to the rendering canvas.
    /// </summary>
    /// <param name="text">The text to add.</param>
    public void Add(Text text)
    {
        text.ShapeChanged += RenderingElement_ShapeChanged;
        RenderingElement_ShapeChanged(text, EventArgs.Empty);
    }

    /// <summary>
    /// Removes the given <paramref name="rectangle"/> from the rendering canvas.
    /// </summary>
    /// <param name="rectangle">The rectangle to add.</param>
    public void Remove(Rectangle rectangle)
    {
        rectangle.ShapeChanged -= RenderingElement_ShapeChanged;
        _transparencyChanged |= _semiTransparentElements.Remove(rectangle);
        _recangles.Remove(rectangle);
    }

    /// <summary>
    /// Removes the given <paramref name="circle"/> from the rendering canvas.
    /// </summary>
    /// <param name="circle">The circle to add.</param>
    public void Remove(Circle circle)
    {
        circle.ShapeChanged -= RenderingElement_ShapeChanged;
        _transparencyChanged |= _semiTransparentElements.Remove(circle);
        _circles.Remove(circle);
    }

    /// <summary>
    /// Removes the given <paramref name="text"/> from the rendering canvas.
    /// </summary>
    /// <param name="text">The text to add.</param>
    public void Remove(Text text)
    {
        text.ShapeChanged -= RenderingElement_ShapeChanged;
        _transparencyChanged |= _semiTransparentElements.Remove(text);
        _texts.Remove(text);
    }

    // TODO: remove the Render() methods once we will have instance rendering!

    /// <summary>
    /// Renders a rectangle.
    /// </summary>
    /// <param name="rectangle">The rectangle shape attributes.</param>
    /// <param name="area">The drawing rectangular area.</param>
    /// <param name="clippingArea">
    /// The area outside of which, pixel shapes won't be rendered.
    /// If <see langword="null" /> no clipping area will be specified.
    /// </param>
    public void Render(Rectangle rectangle)
    {
        RenderShape(rectangle, rectangle.RenderingArea, rectangle.ClipArea, _modelGenerator.Rectangle);
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
    public void Render(Circle circle)
    {
        RenderShape(circle, circle.RenderingArea, circle.ClipArea, _modelGenerator.Circle);
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
    public void Render(Text text)
    {
        // TODO: refactor and optimize. Right now we need to render background rectangle.
        _dispatcher.ThrowIfNotOnUIThread();
        var area = text.RenderingArea;
        var clippingArea = text.ClipArea;

        if (!text.BackgroundColor.IsTransparent)
        {
            var rect = new Rectangle(_dispatcher);
            rect.BackgroundColor = text.BackgroundColor;
            rect.RenderingArea = text.RenderingArea;
            rect.ClipArea = clippingArea;
            rect.ZIndex = text.ZIndex;
            Render(rect);
        }

        if (string.IsNullOrEmpty(text.Value)) return;
        var textKey = $"{text.Value}{text.Font.Size}{text.Font.Identifier}";

        if (!_textCache.TryGetValue(textKey, out var textureId))
        {
            var textureImage = _fontEngine.ConvertTextToGrayscaleImage(text.Value, text.Font, text.ForegroundColor);
            // Do not interpolate (linear filtering) but use narest neighbor for text, to prevent blurry text.
            textureId = CreateTexture(textureImage, interpolate: false);
            _textCache[textKey] = textureId;
        }

        _shader.Use();
        _shader.SetTransform([area.GetTransformMatrix(ViewportSize, 0.0f, 0.0f),
                              area.GetTransformMatrix(ViewportSize, 0.0f, null)]);
        _shader.SetProjection(ViewportSize.GetPorjectionMatrix());
        _shader.SetFillColor(text.ForegroundColor.ToOpenGLColor());
        _shader.SetClipArea((clippingArea ?? new Area(Point.Zero, ViewportSize)).ToVector4(ViewportSize));
        _shader.SetOffsetMultiplier(OpenTK.Mathematics.Vector2.Zero);
        _shader.SetTextureMode(ShaderProgram.TextureMode.GrayScale);
        _shader.SetZIndex(ConvertToInternalZIndex(text.ZIndex).Foreground);
        _shader.SetVisible(text.Visible);
        _modelGenerator.Rectangle.Render(textureId);
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
        _dispatcher.ThrowIfNotOnUIThread();
        _context.MakeCurrent();
        var openGlBackgroundColor = backroundColor.ToOpenGLColor();
        LoggingUtils.OpenGLCheck(() => GL.ClearColor(openGlBackgroundColor.X, openGlBackgroundColor.Y, openGlBackgroundColor.Z, openGlBackgroundColor.W), _logger);
        LoggingUtils.OpenGLCheck(() => GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit), _logger);
        _modelGenerator.ResetDrawCallsCount();
    }

    /// <summary>
    /// This method is called when the frame rendering is complete.
    /// </summary>
    public void FinalizeFrame()
    {
        _dispatcher.ThrowIfNotOnUIThread();
        RenderAllElements(); // TODO: this will be instance rendering
        _context.RenderFrame();
        _logger.LogVerbose($"OpenGL performed {_modelGenerator.DrawCalls} draw calls in the latest frame");
    }

    /// <summary>
    /// Dispose the rendering engine and deallocates all the graphical resources.
    /// </summary>
    public void Shutdown()
    {
        _dispatcher.ThrowIfNotOnUIThread();
        _shader.Close();
    }

    private void RenderShape(Shape shape, Area area, Area? clippingArea, Model2D openglShape)
    {
        // TODO: use barycentric coordinates
        _dispatcher.ThrowIfNotOnUIThread();
        Validate(shape, area);

        var radiusX = 0.0f;
        var radiusY = 0.0f;

        if (shape is Rectangle rectangle)
        {
            radiusX = rectangle.CornerRadiusX ?? 0.0f;
            radiusY = rectangle.CornerRadiusY ?? 0.0f;
        }

        GenericRender(shape.BackgroundColor, area, clippingArea, shape.Rotation, null, radiusX, radiusY, openglShape, ConvertToInternalZIndex(shape.ZIndex).Background, shape.Visible, shape.TextureID);
        GenericRender(shape.BorderColor, area, clippingArea, shape.Rotation, shape.BorderThickness, radiusX, radiusY, openglShape, ConvertToInternalZIndex(shape.ZIndex).Foreground, shape.Visible, shape.TextureID);
    }

    private void GenericRender(Color color,
                               Area area,
                               Area? clippingArea,
                               float rotation,
                               PixelUnit? borderThickness,
                               float xRadius,
                               float yRadius,
                               Model2D openglShape,
                               float zIndex,
                               bool visible,
                               int? textureId)
    {
        // TODO: use barycentric coordinates
        _dispatcher.ThrowIfNotOnUIThread();

        if (color.IsTransparent) return;

        _shader.Use();

        _shader.SetTransform([area.GetTransformMatrix(ViewportSize, rotation, 0.0f),
                              area.GetTransformMatrix(ViewportSize, rotation, borderThickness)]);
        _shader.SetProjection(ViewportSize.GetPorjectionMatrix());
        _shader.SetFillColor(color.ToOpenGLColor());
        _shader.SetClipArea((clippingArea ?? new Area(Point.Zero, ViewportSize)).ToVector4(ViewportSize));
        _shader.SetOffsetMultiplier(NormalizeRadius(xRadius, yRadius, area.Size));
        // TODO: When implementing Texture class, handle the texture format there so we support also gray scale here depending on how the texture was created.
        _shader.SetTextureMode(ShaderProgram.TextureMode.None);
        _shader.SetZIndex(zIndex);
        _shader.SetVisible(visible);
        openglShape.Render(textureId ?? _defaultTexture);
    }

    private void Validate(Shape shape, Area area)
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

    private void RenderAllElements()
    {
        // TODO: use z-buffer in opengl once using render instancing.

        LoggingUtils.OpenGLCheck(() => GL.DepthMask(true), _logger);
        foreach (var rectangle in _recangles)
        {
            Render(rectangle);
        }

        foreach (var circle in _circles)
        {
            Render(circle);
        }

        foreach (var text in _texts)
        {
            Render(text);
        }

        LoggingUtils.OpenGLCheck(() => GL.DepthMask(false), _logger);
        if (_transparencyChanged)
        {
            _backToFrontSortedSemiTransparentElements = _semiTransparentElements.OrderByDescending(e => ConvertToInternalZIndex(e.ZIndex).Background).ToList();
        }

        foreach (var shape in _backToFrontSortedSemiTransparentElements)
        {
            if (!shape.Visible) continue;
            if (shape is Rectangle rectangle) Render(rectangle);
            if (shape is Circle circle) Render(circle);
            if (shape is Text text) Render(text);
        }
        LoggingUtils.OpenGLCheck(() => GL.DepthMask(true), _logger);
        _transparencyChanged = false;
    }

    private void RenderingElement_ShapeChanged(RenderingElement shape, EventArgs e)
    {
        if (!shape.IsSemiTransparent)
        {
            // TODO: add visitor
            _transparencyChanged |= _semiTransparentElements.Remove(shape);
            if (shape is Rectangle rectangle)
            {
                _recangles.Add(rectangle);
            }
            else if (shape is Circle circle)
            {
                _circles.Add(circle);
            }
            else if (shape is Text text)
            {
                _texts.Add(text);
            }
        }
        else
        {
            _transparencyChanged |= _semiTransparentElements.Add(shape);
            if (shape is Rectangle rectangle)
            {
                _recangles.Remove(rectangle);
            }
            else if (shape is Circle circle)
            {
                _circles.Remove(circle);
            }
            else if (shape is Text text)
            {
                _texts.Remove(text);
            }
        }
    }

    // Convert zIndex into a float in the [-1, 1] range suitable for GL_LESS
    // Splits the z index into 2: background first then foreground.
    // TODO: join background and foreground into 1 vbo so no need for this and have less memorys
    internal (float Background, float Foreground) ConvertToInternalZIndex(uint ZIndex)
    {
        var factor = 2u;
        var backgroundZIndex = ZIndex * factor;
        var foregroundZIndex = backgroundZIndex + (factor / 2);
        return (Convert(backgroundZIndex), Convert(foregroundZIndex));
    }

    // Convert zIndex (uint) to a float in the [-1, 1] range suitable for GL_LESS
    private float Convert(uint zIndex)
    {
        // Maximum number of distinct z-values
        const uint maxZIndex = 16777216;

        // Ensure the zIndex is clamped within the valid range
        var clippedZIndex = Math.Min(zIndex, maxZIndex);

        // Convert the zIndex to a float in the [-1, 1] range, reversed
        return 1f - (float)(clippedZIndex) / maxZIndex * 2f;
    }
}
