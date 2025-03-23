using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Shapes;

/// <summary>
/// A circle shape.
/// </summary>
public class Circle : UIShape
{
    private readonly UISharp.Core.Graphics.Shapes.Circle _circle;

    /// <summary>
    /// The circle border color.
    /// </summary>
    public UIProperty<Color> BorderColor { get; }

    /// <summary>
    /// The circle border thickness (0 if not drawing borders).
    /// </summary>
    public UIProperty<PixelUnit> BorderThickness { get; }

    /// <summary>
    /// The circle rotation in radians.
    /// </summary>
    public UIProperty<float> Rotation { get; }

    /// <inheritdoc />
    protected override RenderingElement RenderingElement => _circle;

    /// <summary>
    /// Creates a new circle.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Circle(Application application) : base(application)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();

        _circle = new UISharp.Core.Graphics.Shapes.Circle(application.Dispatcher);

        BorderColor = new UIProperty<Color>(this, application, Color.Transparent);
        BorderThickness = new UIProperty<PixelUnit>(this, application, PixelUnit.Zero);
        Rotation = new UIProperty<float>(this, application, 0.0f);
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
