using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// A rectangle shape with possibly rounded corners.
/// </summary>
public class Rectangle : UIComponent
{
    /// <summary>
    /// The rectangle border color.
    /// </summary>
    public UIProperty<Rectangle, Color> BorderColor { get; }

    /// <summary>
    /// The rectangle border thickness (0 if not drawing borders).
    /// </summary>
    public UIProperty<Rectangle, PixelUnit> BorderThickness { get; }

    /// <summary>
    /// The rectangle corner x-radius in pixels.
    /// </summary>
    public UIProperty<Rectangle, PixelUnit> CornerRadiusX { get; }

    /// <summary>
    /// The rectangle corner y-radius in pixels.
    /// </summary>
    public UIProperty<Rectangle, PixelUnit> CornerRadiusY { get; }

    /// <summary>
    /// The rectangle rotation in radians.
    /// </summary>
    public UIProperty<Rectangle, float> Rotation { get; }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Rectangle(Application application) : base(application, isFocusable: false)
    {
        BorderColor = new UIProperty<Rectangle, Color>(this, Color.Transparent);
        BorderThickness = new UIProperty<Rectangle, PixelUnit>(this, PixelUnit.Zero);
        CornerRadiusX = new UIProperty<Rectangle, PixelUnit>(this, PixelUnit.Zero);
        CornerRadiusY = new UIProperty<Rectangle, PixelUnit>(this, PixelUnit.Zero);
        Rotation = new UIProperty<Rectangle, float>(this, 0.0f);

        RenderFrame += Rectangle_RenderFrame;
    }

    /// <inheritdoc />
    protected override Size ComputeSizeHint() => new(100, 100);

    private void Rectangle_RenderFrame(UIComponent sender, Events.RenderingEventArgs e)
    {
        var rectangleShape = new Graphics.Shapes.Rectangle(BackgroundColor.Value,
                                                           BorderColor.Value,
                                                           BorderThickness.Value,
                                                           CornerRadiusX.Value,
                                                           CornerRadiusY.Value,
                                                           Rotation.Value);
        var canvas = e.Canvas;

        canvas.Render(rectangleShape, RelativeDrawingArea.Fill());
    }
}
