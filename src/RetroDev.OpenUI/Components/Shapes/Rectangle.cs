using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// A rectangle shape with possibly rounded corners.
/// </summary>
public class Rectangle : UIWidget
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
    /// The rectangle corner radius ratio, if <c>1.0f</c> the maximum corner radius will be applied,
    /// if <c>0.0f</c> no corner will be displayed. Note that this only applies to corner radius properties
    /// (<see cref="CornerRadiusX"/> and <see cref="CornerRadiusY"/>) whose value is <see cref="PixelUnit.Auto"/>.
    /// The maximum corner radius is half of the rectangle width or height, whichever is the smallest, and it represents
    /// the maximum corner radius applicable to both <see cref="CornerRadiusX"/> and <see cref="CornerRadiusY"/>.
    /// </summary>
    public UIProperty<Rectangle, float> AutoCornerRadiusRatio { get; }

    /// <summary>
    /// Creates a new rectangle.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Rectangle(Application application) : base(application, isFocusable: false)
    {
        BorderColor = new UIProperty<Rectangle, Color>(this, Color.Transparent);
        BorderThickness = new UIProperty<Rectangle, PixelUnit>(this, PixelUnit.Zero);
        CornerRadiusX = new UIProperty<Rectangle, PixelUnit>(this, PixelUnit.Auto);
        CornerRadiusY = new UIProperty<Rectangle, PixelUnit>(this, PixelUnit.Auto);
        Rotation = new UIProperty<Rectangle, float>(this, 0.0f);
        AutoCornerRadiusRatio = new UIProperty<Rectangle, float>(this, PixelUnit.Zero);

        RenderFrame += Rectangle_RenderFrame;
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        var cornerRadiusX = CornerRadiusX.Value.IsAuto ? PixelUnit.Zero : CornerRadiusX.Value;
        var cornerRadiusY = CornerRadiusY.Value.IsAuto ? PixelUnit.Zero : CornerRadiusY.Value;
        var minimumSizeBasedOnRadius = Math.Max(cornerRadiusX * 2, cornerRadiusY * 2);
        var minimumSizeBasedOnCorner = BorderThickness.Value * 2;
        var minimumSize = Math.Max(minimumSizeBasedOnCorner, minimumSizeBasedOnRadius) + 10;
        return new Size(minimumSize, minimumSize);
    }

    private void Rectangle_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        PixelUnit autoCornerMaximumRadius = Math.Min(ActualSize.Width, ActualSize.Height) / 2.0f;
        PixelUnit autoCornerRadius = autoCornerMaximumRadius * AutoCornerRadiusRatio.Value;

        var cornerRadiusX = CornerRadiusX.Value.IsAuto ? autoCornerRadius : CornerRadiusX.Value;
        var cornerRadiusY = CornerRadiusY.Value.IsAuto ? autoCornerRadius : CornerRadiusY.Value;

        var rectangleShape = new OpenUI.Core.Graphics.Shapes.Rectangle(BackgroundColor.Value,
                                                                       BorderColor.Value,
                                                                       BorderThickness.Value,
                                                                       cornerRadiusX,
                                                                       cornerRadiusY,
                                                                       Rotation.Value);
        var canvas = e.Canvas;

        canvas.Render(rectangleShape, ActualSize.Fill());
    }
}
