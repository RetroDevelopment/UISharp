using RetroDev.OpenUI.Core.Coordinates;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal class Model2D
{
    internal record TriangleAttributes(Point A, Point B, Point C);

    private readonly int _vba;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vertexCount;

    public Model2D(IEnumerable<TriangleAttributes> triangles)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        var indexCounter = 0u;
        var vertexMap = new Dictionary<Point, uint>();

        foreach (var triangle in triangles)
        {
            AddVertexAttribute(triangle.A, vertexMap, vertices, indices, ref indexCounter);
            AddVertexAttribute(triangle.B, vertexMap, vertices, indices, ref indexCounter);
            AddVertexAttribute(triangle.C, vertexMap, vertices, indices, ref indexCounter);
        }

        var vertexArray = vertices.ToArray();
        var indexArray = indices.ToArray();
        _vertexCount = indexArray.Length;

        // Generate and bind the Vertex Array Object (VAO)
        _vba = GL.GenVertexArray();
        GL.BindVertexArray(_vba);

        // Generate and bind the Vertex Buffer Object (VBO)
        _vbo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, vertexArray.Length * sizeof(float), vertexArray, BufferUsageHint.StaticDraw);

        // Generate and bind the Element Buffer Object (EBO)
        _ebo = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer, indexArray.Length * sizeof(uint), indexArray, BufferUsageHint.StaticDraw);

        // Configure the vertex attribute pointers
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        // Unbind the VAO to prevent unintentional modifications
        GL.BindVertexArray(0);
    }

    public void Render(int textureId)
    {
        // Bind the VAO and render the triangles
        GL.BindVertexArray(_vba);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0); // Unbind the VAO after rendering
        GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind texture
    }

    private void AddVertexAttribute(Point vertex, Dictionary<Point, uint> points, List<float> vertices, List<uint> indices, ref uint indexCounter)
    {
        if (points.TryGetValue(vertex, out var index))
        {
            indices.Add(index);
        }
        else
        {
            index = indexCounter++;
            points.Add(vertex, index);
            vertices.AddRange([vertex.X, vertex.Y]);
            indices.Add(index);
        }
    }
}
