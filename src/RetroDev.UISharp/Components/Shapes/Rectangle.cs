using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Shapes;

/// <summary>
/// A rectangle shape with possibly rounded corners.
/// </summary>
public class Rectangle : UIShape
{
    private readonly UISharp.Core.Graphics.Shapes.Rectangle _recangle;

    /// <summary>
    /// The rectangle border color.
    /// </summary>
    public ShapeProperty<Rectangle, Color> BorderColor { get; }

    /// <summary>
    /// The rectangle border thickness (0 if not drawing borders).
    /// </summary>
    public ShapeProperty<Rectangle, PixelUnit> BorderThickness { get; }

    /// <summary>
    /// The rectangle corner x-radius in pixels.
    /// </summary>
    public ShapeProperty<Rectangle, PixelUnit> CornerRadiusX { get; }

    /// <summary>
    /// The rectangle corner y-radius in pixels.
    /// </summary>
    public ShapeProperty<Rectangle, PixelUnit> CornerRadiusY { get; }

    /// <summary>
    /// The rectangle rotation in radians.
    /// </summary>
    public ShapeProperty<Rectangle, float> Rotation { get; }

    /// <inheritdoc />
    protected override RenderingElement RenderingElement => _recangle;

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Rectangle(Application application) : base(application)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();
        _recangle = new(application.Dispatcher);

        BorderColor = new ShapeProperty<Rectangle, Color>(this, application, Color.Transparent);
        BorderThickness = new ShapeProperty<Rectangle, PixelUnit>(this, application, PixelUnit.Zero);
        CornerRadiusX = new ShapeProperty<Rectangle, PixelUnit>(this, application, PixelUnit.Zero);
        CornerRadiusY = new ShapeProperty<Rectangle, PixelUnit>(this, application, PixelUnit.Zero);
        Rotation = new ShapeProperty<Rectangle, float>(this, application, 0.0f);
    }

    /// <summary>
    /// Computes the x-corner radius so that it is the given <paramref name="percentage"/>
    /// of the maximum allowed corner radius for a rectangle with the given <paramref name="size"/>.
    /// </summary>
    /// <param name="percentage">A value from 0 to 1, where 0 is no corner radius and 1 is maximum corner radius.</param>
    /// <param name="size">The rectangle size.</param>
    /// <returns>The x-corner radius in pixels.</returns>
    /// <exception cref="ArgumentException">If <paramref name="size"/> is not between 0 and 1.</exception>
    public PixelUnit ComputeXCornerRadius(float percentage, Size size)
    {
        if (percentage < 0.0f || percentage > 1.0f)
        {
            throw new ArgumentException($"Corner x radius percentage must be between 0 and 1: {percentage} found");
        }

        var maximumXCornerRadius = size.Width / 2.0f;
        return maximumXCornerRadius * percentage;
    }

    /// <summary>
    /// Computes the y-corner radius so that it is the given <paramref name="percentage"/>
    /// of the maximum allowed corner radius for a rectangle with the given <paramref name="size"/>.
    /// </summary>
    /// <param name="percentage">A value from 0 to 1, where 0 is no corner radius and 1 is maximum corner radius.</param>
    /// <param name="size">The rectangle size.</param>
    /// <returns>The y-corner radius in pixels.</returns>
    /// <exception cref="ArgumentException">If <paramref name="size"/> is not between 0 and 1.</exception>
    public PixelUnit ComputeYCornerRadius(float percentage, Size size)
    {
        if (percentage < 0.0f || percentage > 1.0f)
        {
            throw new ArgumentException($"Corner x radius percentage must be between 0 and 1: {percentage} found");
        }

        var maximumYCornerRadius = size.Height / 2.0f;
        return maximumYCornerRadius * percentage;
    }

    /// <summary>
    /// Computes the corner radius so that it is the given <paramref name="percentage"/>
    /// of the maximum allowed corner radius for a rectangle with the given <paramref name="size"/>.
    /// </summary>
    /// <param name="percentage">A value from 0 to 1, where 0 is no corner radius and 1 is maximum corner radius.</param>
    /// <param name="size">The rectangle size.</param>
    /// <returns>The corner radius in pixels.</returns>
    /// <exception cref="ArgumentException">If <paramref name="size"/> is not between 0 and 1.</exception>
    public PixelUnit ComputeCornerRadius(float percentage, Size size) =>
        Math.Min(ComputeXCornerRadius(percentage, size), ComputeYCornerRadius(percentage, size));

    protected internal override void AddShapeToRenderingEngine()
    {
        RenderingEngine?.Add(_recangle);
    }

    protected internal override void RemoveShapeFromRenderingEngine()
    {
        RenderingEngine?.Remove(_recangle);
    }

    protected internal override void Render()
    {
        _recangle.BorderColor = BorderColor.Value;
        _recangle.BorderThickness = BorderThickness.Value;
        _recangle.CornerRadiusX = CornerRadiusX.Value;
        _recangle.CornerRadiusY = CornerRadiusY.Value;
        _recangle.Rotation = Rotation.Value;
    }
}
