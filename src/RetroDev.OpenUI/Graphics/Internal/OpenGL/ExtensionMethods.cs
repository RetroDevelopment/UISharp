using System.Runtime.CompilerServices;
using OpenTK.Graphics.ES20;
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
    /// <returns>A transofmation matrix to pass as uniform to the vertex shader.</returns>
    public static Matrix3 GetTransformMatrix(this Area renderingArea, Size viewportSize)
    {
        var transform = Matrix3.Identity;

        var scale = new Vector2(renderingArea.Size.Width / viewportSize.Width, renderingArea.Size.Height / viewportSize.Height);
        var translate = renderingArea.Center.ToOpenGLPoint(viewportSize);

        transform.M11 = scale.X;
        transform.M22 = scale.Y;
        transform.M31 = translate.X;
        transform.M32 = translate.Y;

        return transform;
    }

    /// <summary>
    /// Transforms <see langword="this"/> area into OpenGL coordinate system area.
    /// </summary>
    /// <param name="area">The area to transform.</param>
    /// <param name="viewportSize">The viewport size.</param>
    /// <returns></returns>
    public static Vector4 ToOpenGLArea(this Area area, Size viewportSize)
    {
        var openGLTopLeft = area.TopLeft.ToOpenGLPoint(viewportSize);
        var openGLBottomRight = area.BottomRight.ToOpenGLPoint(viewportSize);

        return new(openGLTopLeft.X, openGLTopLeft.Y, openGLBottomRight.X, openGLBottomRight.Y);
    }

    public static Vector2 ToOpenGLPoint(this Point point, Size viewportSize) =>
        new(-1.0f + (point.X / viewportSize.Width) * 2.0f,
            1.0f - (point.Y / viewportSize.Height) * 2.0f);

    /// <summary>
    /// Converts <see langword="this"/> <see cref="Color"/> from rgba into opengl format.
    /// </summary>
    /// <param name="color">The rgba color to convert.</param>
    /// <returns>A OpenGL color, where each component is from 0 (darkest) to 1 (brightest).</returns>
    public static Vector4 ToOpenGLColor(this Color color) =>
        new(color.Red / 255.0f, color.Green / 255.0f, color.Blue / 255.0f, color.Alpha / 255.0f);
}
