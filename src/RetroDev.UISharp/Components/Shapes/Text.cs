using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Shapes;

/// <summary>
/// Displays test in the UI.
/// </summary>
public class Text : UIShape
{
    private readonly UISharp.Core.Graphics.Shapes.Text _text;

    /// <summary>
    /// The text color.
    /// </summary>
    public ShapeProperty<Text, Color> TextColor { get; }

    /// <summary>
    /// The text to display.
    /// </summary>
    public ShapeProperty<Text, string> DisplayText { get; }

    /// <summary>
    /// The <see cref="DisplayText"/> <see cref="Font"/>.
    /// </summary>
    public ShapeProperty<Text, Font> Font { get; }

    /// <inheritdoc />
    protected override RenderingElement RenderingElement => _text;

    /// <summary>
    /// Creates a new text.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Text(Application application) : base(application)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();

        _text = new(application.Dispatcher, application.DefaultFont.Value.ToGraphicsFont());

        TextColor = new ShapeProperty<Text, Color>(this, application, Color.Transparent);
        DisplayText = new ShapeProperty<Text, string>(this, application, string.Empty);
        Font = new ShapeProperty<Text, Font>(this, application, application.DefaultFont, BindingType.DestinationToSource);
    }

    /// <summary>
    /// The minimum size required to render the text correctly.
    /// </summary>
    /// <returns>
    /// The text size, which is <see cref="Size.Zero"/> if the text has not yet been attached to a <see cref="Canvas"/> or
    /// the <see cref="UIComponent"/> owning <see langword="this" /> text is not yet assigned to a window or a component with a rendering viewport.
    /// </returns>
    public Size ComputeTextSize() =>
        RenderingEngine?.ComputeTextSize(DisplayText.Value, Font.Value.ToGraphicsFont()) ?? Size.Zero;

    /// <summary>
    /// Gets the maximum height occupied in a line of text with the given <see cref="Font"/>.
    /// </summary>
    /// <returns>
    /// The text height, which is <see cref="PixelUnit.Zero"/> if the text has not yet been attached to a <see cref="Canvas"/> or
    /// the <see cref="UIComponent"/> owning <see langword="this" /> text is not yet assigned to a window or a component with a rendering viewport.
    /// </returns>
    public PixelUnit ComputeTextMaximumHeight() =>
        RenderingEngine?.ComputeTextMaximumHeight(Font.Value.ToGraphicsFont()) ?? PixelUnit.Zero;

    protected internal override void AddShapeToRenderingEngine()
    {
        RenderingEngine?.Add(_text);
    }

    protected internal override void RemoveShapeFromRenderingEngine()
    {
        RenderingEngine?.Remove(_text);
    }

    protected internal override void Render()
    {
        _text.RenderingArea = ComputeTextSize().PositionCenterOf(RelativeRenderingArea.Value.Size).ToAbsolute(Canvas.ContainerAbsoluteDrawingArea);
        _text.ForegroundColor = TextColor.Value;
        _text.Value = DisplayText.Value;
        _text.Font = Font.Value.ToGraphicsFont();
    }
}
