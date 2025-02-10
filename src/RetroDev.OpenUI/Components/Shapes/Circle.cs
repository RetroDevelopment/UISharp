using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// A circle shape.
/// </summary>
public class Circle : UIWidget
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
    /// The circle radius. By default it is <see cref="PixelUnit.Auto"/>, which it will set <see cref="UIComponent.Width"/>
    /// and <see cref="UIComponent.Height"/> to <see cref="PixelUnit.Auto"/>. By default, that will make the circle to stretch
    /// to the container. If <see cref="Radius"/> is not <see cref="PixelUnit.Auto"/>, <see cref="UIComponent.Width"/> and
    /// <see cref="UIComponent.Height"/> will be set accordingly to render a circle with the given radius.
    /// </summary>
    public UIProperty<Circle, PixelUnit> Radius { get; }

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
        Radius = new UIProperty<Circle, PixelUnit>(this, PixelUnit.Auto);
        Rotation = new UIProperty<Circle, float>(this, 0.0f);

        Radius.ValueChange += Radius_ValueChange;

        RenderFrame += Rectangle_RenderFrame;
    }

    private void Radius_ValueChange(BindableProperty<PixelUnit> sender, ValueChangeEventArgs<PixelUnit> e)
    {
        if (e.CurrentValue.IsAuto)
        {
            Width.Value = PixelUnit.Auto;
            Height.Value = PixelUnit.Auto;
        }
        else
        {
            var diameter = e.CurrentValue.Value * 2.0f;
            Width.Value = diameter;
            Height.Value = diameter;
        }
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> chilrenSize)
    {
        var minimumSize = BorderThickness.Value * 2 + 10; // Display at least border.
        return new Size(minimumSize, minimumSize);
    }

    private void Rectangle_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var circleShape = new OpenUI.Core.Graphics.Shapes.Circle(BackgroundColor.Value,
                                                                 BorderColor.Value,
                                                                 BorderThickness.Value,
                                                                 Rotation.Value);
        var canvas = e.Canvas;

        canvas.Render(circleShape, ActualSize.Fill());
    }
}
