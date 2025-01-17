using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Logging;
using RetroDev.OpenUI.Utils;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal class ShaderProgram
{
    private readonly int _id;

    public ShaderProgram(IEnumerable<Shader> shaders, ILogger logger)
    {
        _id = GL.CreateProgram();
        foreach (var shader in shaders)
        {
            GL.AttachShader(_id, shader.ID);
        }

        GL.LinkProgram(_id);
        LoggingUtils.LogShaderStatus($"linking shaders", _id, logger);

        foreach (var shader in shaders)
        {
            shader.Close();
        }
    }

    public void Use()
    {
        GL.UseProgram(_id);
    }

    public void SetTransform(List<Matrix3> transform) =>
        SetMatrix3Array("transforms", transform);

    public void SetProjection(Matrix3 transform) =>
        SetMatrix3("projection", transform);

    public void SetFillColor(Vector4 color) =>
        SetVec4("color", color);

    public void SetClipArea(Vector4 clipArea) =>
        SetVec4("clipArea", clipArea);

    public void SetOffsetMultiplier(Vector2 multiplier) =>
        SetVec2("offsetMultiplier", multiplier);

    public void Close()
    {
        GL.DeleteProgram(_id);
    }

    private void SetFloat(string name, float value) =>
        GL.Uniform1(GetUniformLocation(name), value);

    private void SetMatrix3(string name, Matrix3 matrix) =>
        GL.UniformMatrix3(GetUniformLocation(name), 1, false, ref matrix.Row0.X);

    private void SetMatrix3Array(string name, List<Matrix3> matrices)
    {
        // Get the uniform location
        const int maxtrixSize = 9;
        var location = GetUniformLocation(name);

        // Flatten the matrices into a single array of floats
        var matrixData = new float[matrices.Count * maxtrixSize];
        for (var i = 0; i < matrices.Count; i++)
        {
            var matrix = matrices[i];
            matrixData[i * maxtrixSize + 0] = matrix.Row0.X;
            matrixData[i * maxtrixSize + 1] = matrix.Row0.Y;
            matrixData[i * maxtrixSize + 2] = matrix.Row0.Z;
            matrixData[i * maxtrixSize + 3] = matrix.Row1.X;
            matrixData[i * maxtrixSize + 4] = matrix.Row1.Y;
            matrixData[i * maxtrixSize + 5] = matrix.Row1.Z;
            matrixData[i * maxtrixSize + 6] = matrix.Row2.X;
            matrixData[i * maxtrixSize + 7] = matrix.Row2.Y;
            matrixData[i * maxtrixSize + 8] = matrix.Row2.Z;
        }

        // Pass the array of matrices to OpenGL
        GL.UniformMatrix3(location, matrices.Count, false, matrixData);
    }

    private void SetVec2(string name, Vector2 vec) =>
        GL.Uniform2(GetUniformLocation(name), vec);

    private void SetVec4(string name, Vector4 vec) =>
        GL.Uniform4(GetUniformLocation(name), vec);

    private int GetUniformLocation(string name)
    {
        var uniformLocation = GL.GetUniformLocation(_id, name);
        if (uniformLocation == -1) throw new RenderingException($"Cannot find uniform {name}");

        return uniformLocation;
    }
}
