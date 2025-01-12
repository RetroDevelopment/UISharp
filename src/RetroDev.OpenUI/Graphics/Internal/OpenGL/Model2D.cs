using RetroDev.OpenUI.Core.Coordinates;
using OpenTK.Graphics.OpenGL;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal class Model2D
{
    internal record VertexAttribute(Point Location, Point? Offset = null, uint TransformIndex = 0)
    {
        public VertexAttribute Duplicate(uint transformIndex) => new(Location, Offset, transformIndex);
        public static List<VertexAttribute> Generate(Point location, double fromAngle, double toAngle, float signX, float signY)
        {
            var angleStepRadians = 2.0 * Math.PI / 360.0; // Draw a triangle for each 1 degree angle
            List<VertexAttribute> attributes = [];
            for (var angle = fromAngle; angle > toAngle; angle -= angleStepRadians)
            {
                // The formula is obtained as follows: let's say the top left corner point A to split into multiple points generating a corner radius.
                // Given a radius for x (rx) and a radius for y (ry) of rounded corners, the center of the corner CA is (Ax + rx, Ay - ry).
                // So, the point P in the corner with angle alpha is 
                // P = (Cx + rx * cos(alpha), Cy + ry * cos(alpha))
                //   = (Ax + rx * cos(alpha), Ay - ry * sin(alpha))
                //   = (AX + rx * (1 + cos(alpha), Ay + ry * (-1 + sin(alpha))
                // Because the vertex shader calculates radius using
                // P = A + multiplier * offset
                // the vbo offset will be (substituting the formulas) offset = (1 + cos(alpha), -1 + sin(alpha)) for the top-left point A.
                // Signs will change depending on the corner (top-right, bottom-right, etc.)
                attributes.Add(new VertexAttribute(location, new Point(signX * 1.0f + (float)Math.Cos(angle), signY * 1.0f + (float)Math.Sin(angle))));
            }

            return attributes;
        }
    }

    private readonly int _vba;
    private readonly int _vbo;
    private readonly int _ebo;
    private readonly int _vertexCount;

    /// <summary>
    /// Creates a new 2D model which generates a shape with the given <paramref name="points"/> which can be configured as a border or fill shape.
    /// </summary>
    /// <param name="points">The list of model vertices. THE GENERATED SHAPE BARICENTER MUST BE (0, 0) IN CARTESIAN COORDINATES.</param>
    public Model2D(List<VertexAttribute> points)
    {
        var vertices = new List<float>();
        var indices = new List<uint>();
        var indexCounter = 0u;
        var vertexMap = new Dictionary<VertexAttribute, uint>();

        var vertexAttributes = CreateVertexAttributes(points);

        foreach (var vertexAttrbiute in vertexAttributes)
        {
            AddVertexAttribute(vertexAttrbiute, vertexMap, vertices, indices, ref indexCounter);
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
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0); // position
        GL.VertexAttribPointer(1, 1, VertexAttribPointerType.Float, false, 5 * sizeof(float), 2 * sizeof(float)); // index
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float)); // offset

        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        // Unbind the VAO to prevent unintentional modifications
        GL.BindVertexArray(0);
    }

    public void Render(int textureId)
    {
        // Bind the VAO and render the triangles
        GL.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        GL.BindVertexArray(_vba);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, textureId);
        GL.DrawElements(PrimitiveType.Triangles, _vertexCount, DrawElementsType.UnsignedInt, 0);

        GL.BindVertexArray(0); // Unbind the VAO after rendering
        GL.BindTexture(TextureTarget.Texture2D, 0); // Unbind texture
    }

    private void AddVertexAttribute(VertexAttribute vertexAttrubute,
                                    Dictionary<VertexAttribute, uint> vertexAttributeMap,
                                    List<float> openGLAttributes,
                                    List<uint> openGLIndices,
                                    ref uint indexCounter)
    {
        if (vertexAttributeMap.TryGetValue(vertexAttrubute, out var index))
        {
            openGLIndices.Add(index);
        }
        else
        {
            index = indexCounter++;
            vertexAttributeMap.Add(vertexAttrubute, index);
            openGLAttributes.AddRange([vertexAttrubute.Location.X, vertexAttrubute.Location.Y, vertexAttrubute.TransformIndex, vertexAttrubute.Offset?.X ?? 0, vertexAttrubute.Offset?.Y ?? 0]);
            openGLIndices.Add(index);
        }
    }

    private List<VertexAttribute> CreateVertexAttributes(List<VertexAttribute> points)
    {
        var result = new List<VertexAttribute>();
        var numberOfPoints = points.Count;

        //if (numberOfPoints <= 1) throw new ArgumentException($"Only provided {numberOfPoints} points, at least 2 points expected");

        for (int i = 0; i < numberOfPoints; i++)
        {
            var pointA = points[i];
            var pointB = points[(i + 1) % numberOfPoints];

            var outerPointA = pointA;
            var outerPointB = pointB;
            var innerPointA = pointA.Duplicate(1);
            var innerPointB = pointB.Duplicate(1);

            // Join quat points as two trinagles (we are using OpenGL triangles primitive).
            result.AddRange([outerPointA, outerPointB, innerPointB]);
            result.AddRange([innerPointB, innerPointA, outerPointA]);
        }

        return result;
    }
}
