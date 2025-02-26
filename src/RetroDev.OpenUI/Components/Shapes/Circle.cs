using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Graphics.Shapes;
using RetroDev.OpenUI.Presentation.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// A circle shape.
/// </summary>
public class Circle : UIShape
{
    private readonly OpenUI.Core.Graphics.Shapes.Circle _circle;

    /// <summary>
    /// The circle border color.
    /// </summary>
    public ShapeProperty<Circle, Color> BorderColor { get; }

    /// <summary>
    /// The circle border thickness (0 if not drawing borders).
    /// </summary>
    public ShapeProperty<Circle, PixelUnit> BorderThickness { get; }

    /// <summary>
    /// The circle rotation in radians.
    /// </summary>
    public ShapeProperty<Circle, float> Rotation { get; }

    /// <inheritdoc />
    protected override RenderingElement RenderingElement => _circle;

    /// <summary>
    /// Creates a new circle.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Circle(Application application) : base(application)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();

        _circle = new OpenUI.Core.Graphics.Shapes.Circle(application.Dispatcher);

        BorderColor = new ShapeProperty<Circle, Color>(this, application, Color.Transparent);
        BorderThickness = new ShapeProperty<Circle, PixelUnit>(this, application, PixelUnit.Zero);
        Rotation = new ShapeProperty<Circle, float>(this, application, 0.0f);
    }

    /// <summary>
    /// Converts the given <paramref name="radius"/> into the <see cref="Size"/> of <see langword="this" /> <see cref="Circle"/>.
    /// </summary>
    /// <param name="radius">The circle radius.</param>
    /// <returns>The <see cref="Size"/> for <see langword="this" /> <see cref="Circle"/> if it has to have the givne <paramref name="radius"/>.</returns>
    public Size RadiusToSize(PixelUnit radius) => new(radius * 2.0f, radius * 2.0f);

    protected internal override void AddShapeToRenderingEngine()
    {
        RenderingEngine?.Add(_circle);
    }

    protected internal override void RemoveShapeFromRenderingEngine()
    {
        RenderingEngine?.Remove(_circle);
    }

    protected internal override void Render()
    {
        _circle.BorderColor = BorderColor.Value;
        _circle.BorderThickness = BorderThickness.Value;
        _circle.Rotation = Rotation.Value;
    }
}
