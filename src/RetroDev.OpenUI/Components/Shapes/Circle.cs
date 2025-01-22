using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// A circle shape.
/// </summary>
public class Circle : UIComponent
{
    /// <summary>
    /// The circle border color.
    /// </summary>
    public UIProperty<Circle, Color> BorderColor { get; }

    /// <summary>
    /// The circle border thickness (0 if not drawing borders).
    /// </summary>
    public UIProperty<Circle, PixelUnit> BorderThickness { get; }

    /// <summary>
    /// The circle rotation in radians.
    /// </summary>
    public UIProperty<Circle, float> Rotation { get; }

    /// <summary>
    /// Creates a new circle.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Circle(Application application) : base(application, isFocusable: false)
    {
        BorderColor = new UIProperty<Circle, Color>(this, Color.Transparent);
        BorderThickness = new UIProperty<Circle, PixelUnit>(this, PixelUnit.Zero);
        Rotation = new UIProperty<Circle, float>(this, 0.0f);

        RenderFrame += Rectangle_RenderFrame;
    }

    /// <inheritdoc />
    protected override Size ComputeSizeHint() => new(100, 100);

    private void Rectangle_RenderFrame(UIComponent sender, Events.RenderingEventArgs e)
    {
        var rectangleShape = new Graphics.Shapes.Circle(BackgroundColor.Value,
                                                        BorderColor.Value,
                                                        BorderThickness.Value,
                                                        Rotation.Value);
        var canvas = e.Canvas;

        canvas.Render(rectangleShape, RelativeDrawingArea.Fill());
    }
}
