using RetroDev.UISharp.Core.Graphics.Coordinates;
using RetroDev.UISharp.Core.Windowing;

namespace RetroDev.UISharp.Core.Graphics.Shapes;

/// <param name="CornerRadiusX">
/// The rectangle corner x-radius in pixels.
/// If omitted or zero, no rounded corners will be displyed.
/// </param>
/// <param name="CornerRadiusY">
/// The rectangle corner y-radius in pixels.
/// If omitted or zero, no rounded corners will be displyed.
/// </param>
/// <param name="TextureID">
/// The ID of the texture to display, or <see langword="null" /> if no texture is specified.
/// </param>
public class Rectangle(ThreadDispatcher dispatcher) : Shape(dispatcher)
{
    private PixelUnit _cornerRadiusX = PixelUnit.Zero;
    private PixelUnit _cornerRadiusY = PixelUnit.Zero;

    /// <summary>
    /// The rectangle corner x-radius in pixels.
    /// </summary>
    public PixelUnit CornerRadiusX
    {
        get => _cornerRadiusX;
        set => SetValue(ref _cornerRadiusX, value);
    }

    /// <summary>
    /// The rectangle corner y-radius in pixels.
    /// </summary>
    public PixelUnit CornerRadiusY
    {
        get => _cornerRadiusY;
        set => SetValue(ref _cornerRadiusY, value);
    }
}
