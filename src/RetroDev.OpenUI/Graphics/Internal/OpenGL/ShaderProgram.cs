using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RetroDev.OpenUI.Exceptions;
using SkiaSharp;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal class ShaderProgram
{
    private readonly int _id;

    public ShaderProgram(IEnumerable<Shader> shaders)
    {
        _id = GL.CreateProgram();
        foreach (var shader in shaders)
        {
            GL.AttachShader(_id, shader.ID);
        }

        GL.LinkProgram(_id);
        Console.WriteLine("Program logs: " + GL.GetProgramInfoLog(_id));

        foreach (var shader in shaders)
        {
            shader.Close();
        }
    }

    public void Use()
    {
        GL.UseProgram(_id);
    }

    public void SetTransform(Matrix3 transform) =>
        SetMatrix3("transform", transform);

    public void SetFillColor(Vector4 color) =>
        SetVec4("color", color);

    public void SetClipArea(Vector4? clipArea) =>
        SetVec4("clipArea", clipArea ?? new Vector4(-1.0f, 1.0f, 1.0f, -1.0f));

    public void Close()
    {
        GL.DeleteProgram(_id);
    }

    private void SetMatrix3(string name, Matrix3 matrix) =>
        GL.UniformMatrix3(GetUniformLocation(name), 1, false, ref matrix.Row0.X);

    private void SetVec4(string name, Vector4 vec) =>
        GL.Uniform4(GetUniformLocation(name), vec);

    private int GetUniformLocation(string name)
    {
        var uniformLocation = GL.GetUniformLocation(_id, name);
        if (uniformLocation == -1) throw new RenderingException($"Cannot find uniform {name}");

        return uniformLocation;
    }
}
