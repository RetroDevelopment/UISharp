using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Windowing;

namespace RetroDev.UISharp.Core.Graphics.Shapes;

/// <summary>
/// Defines a generic shape to render.
/// This shape enables retained mode rendering, so it is meant to be stored in a <see cref="IRenderingEngine"/> and
/// modify its values whenever somethign changes, instead of creating a new shape for each frame.
/// </summary>
/// <param name="dispatcher">Manages the UI thread and dispatches UI operations from other thread to the UI thread.</param>
public abstract class Shape(ThreadDispatcher dispatcher) : RenderingElement(dispatcher)
{
    private Color _borderColor = Color.Transparent;
    private PixelUnit _borderThickness = PixelUnit.Zero;
    private float _rotation = 0.0f;
    private int? _textureId = 0;

    /// <summary>
    /// Whether the shape is not opaque but not transparent either.
    /// </summary>
    public override bool IsSemiTransparent =>
        base.IsSemiTransparent || BorderColor.IsSemiTransparent;

    /// <summary>
    /// The shape border color.
    /// </summary>
    public Color BorderColor
    {
        get => _borderColor;
        set => SetValue(ref _borderColor, value);
    }

    /// <summary>
    /// The border thickness in pixels.
    /// </summary>
    public PixelUnit BorderThickness
    {
        get => _borderThickness;
        set => SetValue(ref _borderThickness, value);
    }

    /// <summary>
    /// The image rotation in radians.
    /// </summary>
    public float Rotation
    {
        get => _rotation;
        set => SetValue(ref _rotation, value);
    }

    /// <summary>
    /// The ID of the texture to display, or <see langword="null" /> if no texture is specified.
    /// </summary>
    public int? TextureID
    {
        get => _textureId;
        set => SetValue(ref _textureId, value);
    }
}
