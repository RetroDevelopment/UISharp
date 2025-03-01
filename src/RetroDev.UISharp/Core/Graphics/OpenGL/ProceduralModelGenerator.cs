using RetroDev.UISharp.Core.Coordinates;
using static RetroDev.UISharp.Core.Graphics.OpenGL.Model2D;

namespace RetroDev.UISharp.Core.Graphics.OpenGL;

internal class ProceduralModelGenerator
{
    public Model2D Rectangle { get; }
    public Model2D Circle { get; }

    public int DrawCalls => Rectangle._drawCalls + Circle._drawCalls;

    public ProceduralModelGenerator()
    {
        Rectangle = CreateRectangleModel();
        Circle = CreateCircleModel();
    }

    public void ResetDrawCallsCount()
    {
        Rectangle._drawCalls = 0;
        Circle._drawCalls = 0;
    }

    // The idea around rectangle is the following: each point will be added by the offset (see VertexAttribute.Generate and vertex shader).
    // So a point P in the rectangle will become
    // Poffset = P + multipler * offset
    // In the vertex shader multiplier is a vector representing corner radius x and y. Now, if multiplier = (0.5, 0.5) corner radius will be maximum (square has size 1
    // so 2 radius of 0.5 make a semi-circle). The code below generates the various corner points by splitting EACH OF THE 4 POINTS into multiple points with the SAME COORDINATES AS THE ORIGINAL POINT
    // but different offset, in a way that, given a multiplier, it is possible to dynamically determine the corner radius. This is for performance reason, to avoid making milion VBO with different corner radius.
    // 
    // Now, there is also border generation (see Model2D implementation). This is done by taking all these points and duplicating them and applying a different transform matrix for outer and inner points.
    // Baseically, the outer points (say the original points) have the original transformation matrix and the inner points (say the duplicated) have a
    // transformation matrix with a proportionally smaller scale. This allows creating borders (see it is done automatically inside the Model2D for each model).
    // BE AWARE THAT THIS METHOD of scaling down the duplicate image ONLY WORKS IF BARICENTER OF THE VBO IS IN (0, 0).
    // THIS IS NOT the case for ROUNDED CORNERS rectangles, but it is close enough not to be a big problem in practice.
    // Howver, in the future we should consider taking into account barycenter for more accurate figures.
    private Model2D CreateRectangleModel()
    {
        List<VertexAttribute> points = [];
        var topLeftRoundedCornerPoints = VertexAttribute.Generate(new Point(-0.5f, 0.5f), Math.PI, Math.PI / 2.0, 1.0f, -1.0f);
        var topRightRoundedCornerPoints = VertexAttribute.Generate(new Point(0.5f, 0.5f), Math.PI / 2.0, 0.0f, -1.0f, -1.0f);
        var bottomRightRoundedCornerPoints = VertexAttribute.Generate(new Point(0.5f, -0.5f), 0.0f, -Math.PI / 2.0, -1.0f, 1.0f);
        var bottomLeftRoundedCornerPoints = VertexAttribute.Generate(new Point(-0.5f, -0.5f), -Math.PI / 2.0, -Math.PI, 1.0f, 1.0f);

        points.AddRange(topLeftRoundedCornerPoints);
        points.AddRange(topRightRoundedCornerPoints);
        points.AddRange(bottomRightRoundedCornerPoints);
        points.AddRange(bottomLeftRoundedCornerPoints);

        return new Model2D(points);
    }

    private Model2D CreateCircleModel()
    {
        var points = new List<VertexAttribute>();
        var angleStepRadians = 2.0 * Math.PI / 360.0; // Draw a triangle for each 1 degree angle

        for (var angle = 0.0; angle < 2.0 * Math.PI; angle += angleStepRadians)
        {
            points.Add(new VertexAttribute(new Point((float)Math.Cos(angle) / 2.0f, (float)Math.Sin(angle) / 2.0f)));
        }

        return new Model2D(points);
    }
}
