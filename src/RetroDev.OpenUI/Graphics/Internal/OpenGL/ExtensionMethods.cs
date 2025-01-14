using OpenTK.Mathematics;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal static class ExtensionMethods
{
    /// <summary>
    /// Gets a transform matrix to scale a <see cref="Model2D"/> (defined in the -1, 1 x and y OpenGL
    /// coordinate space) so that it is rendered in the given <paramref name="renderingArea"/>.
    /// </summary>
    /// <param name="renderingArea">The location and size of the area where to render a <see cref="Model2D"/>.</param>
    /// <param name="viewportSize">The size of the full rendering area.</param>
    /// <param name="rotation">The model rotation in radians.</param>
    /// <param name="borderThickness">
    /// The figure border thickness, used to reduce the scale for inner border.
    /// If <see langword="null" />, the scale is set to 0.
    /// </param>
    /// <returns>A transofmation matrix to pass as uniform to the vertex shader.</returns>
    public static Matrix3 GetTransformMatrix(this Area renderingArea,
                                             Size viewportSize,
                                             float rotation,
                                             PixelUnit? borderThickness)
    {
        var transform = Matrix3.Identity;

        Vector2 scale;

        if (borderThickness != null)
        {
            scale = new Vector2(renderingArea.Size.Width - borderThickness,
                                renderingArea.Size.Height - borderThickness);
        }
        else
        {
            scale = Vector2.Zero;
        }

        var translate = renderingArea.Center.FromScreenToCartesian(viewportSize);

        transform.M11 = scale.X * (float)Math.Cos(rotation);
        transform.M21 = -scale.Y * (float)Math.Sin(rotation);
        transform.M12 = scale.X * (float)(Math.Sin(rotation));
        transform.M22 = scale.Y * (float)(Math.Cos(rotation));
        transform.M31 = translate.X;
        transform.M32 = translate.Y;

        return transform;
    }

    /// <summary>
    /// Gets a projection matrix to convert from screen coordinates into OpenGL NDC.
    /// </summary>
    /// <param name="viewportSize">The size of the full rendering area.</param>
    /// <returns>The projection matrix used to convert from screen coordinates into OpenGL NDC.</returns>
    public static Matrix3 GetPorjectionMatrix(this Size viewportSize)
    {
        var projection = Matrix3.Identity;

        projection.M11 = 2.0f / viewportSize.Width;
        projection.M22 = 2.0f / viewportSize.Height;

        return projection;
    }

    /// <summary>
    /// Transforms <see langword="this"/> area into a cartesian coordinate area represented as <see cref="Vector4"/>.
    /// </summary>
    /// <param name="area">The area to transform.</param>
    /// <param name="viewportSize">The viewport size.</param>
    /// <returns>A vector with the following cartesian coordinates [topLeft.x, topLeft.y, bottomRight.x, bottomRight.y].</returns>
    public static Vector4 ToVector4(this Area area, Size viewportSize)
    {
        var cartesianTopLeft = area.TopLeft.FromScreenToCartesian(viewportSize);
        var cartesianBottomRight = area.BottomRight.FromScreenToCartesian(viewportSize);
        return new Vector4(cartesianTopLeft.X, cartesianTopLeft.Y, cartesianBottomRight.X, cartesianBottomRight.Y);
    }

    /// <summary>
    /// Converts <see langword="this"/> <see cref="Color"/> from rgba into opengl format.
    /// </summary>
    /// <param name="color">The rgba color to convert.</param>
    /// <returns>A OpenGL color, where each component is from 0 (darkest) to 1 (brightest).</returns>
    public static Vector4 ToOpenGLColor(this Color color) =>
        new(color.RedComponent / 255.0f, color.GreenComponent / 255.0f, color.BlueComponent / 255.0f, color.AlphaComponent / 255.0f);
}
