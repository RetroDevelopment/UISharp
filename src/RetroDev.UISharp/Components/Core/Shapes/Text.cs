using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Shapes;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Binding;

namespace RetroDev.UISharp.Components.Core.Shapes;

/// <summary>
/// Displays test in the UI.
/// </summary>
public class Text : UIShape
{
    private readonly UISharp.Core.Graphics.Shapes.Text _text;

    /// <summary>
    /// The text color.
    /// </summary>
    public UIProperty<Color> TextColor { get; }

    /// <summary>
    /// The text to display.
    /// </summary>
    public UIProperty<string> DisplayText { get; }

    /// <summary>
    /// The <see cref="DisplayText"/> <see cref="Font"/>.
    /// </summary>
    public UIProperty<Font> Font { get; }

    /// <summary>
    /// The text horizontal alignment within the text boundaries.
    /// </summary>
    public UIProperty<IHorizontalAlignment> TextHorizontalAlignment { get; }

    /// <summary>
    /// The text vertical alignment within the text boundaries.
    /// </summary>
    public UIProperty<IVerticalAlignment> TextVerticalAlignment { get; }

    /// <summary>
    /// An horizontal offset to apply to text after all positioning calculation.
    /// That is useful for implementing scrollable text.
    /// </summary>
    public UIProperty<PixelUnit> HorizontalScroll { get; }

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

        TextColor = new UIProperty<Color>(this, application, Color.Transparent);
        DisplayText = new UIProperty<string>(this, application, string.Empty);
        Font = new UIProperty<Font>(this, application, application.DefaultFont, BindingType.SourceToDestination);
        TextHorizontalAlignment = new UIProperty<IHorizontalAlignment>(this, application, Alignment.Left);
        TextVerticalAlignment = new UIProperty<IVerticalAlignment>(this, application, Alignment.Center);
        HorizontalScroll = new UIProperty<PixelUnit>(this, application, PixelUnit.Zero);
    }

    /// <summary>
    /// The minimum size required to render the text correctly.
    /// </summary>
    /// <param name="index">
    /// The truncation index. If <see langword="null" /> the full text size will be computed, otherwise only the size of the text
    /// untile <paramref name="index"/> will be computed.
    /// </param>
    /// <returns>
    /// The text size, which is <see cref="Size.Zero"/> if the text has not yet been attached to a <see cref="Canvas"/> or
    /// the <see cref="UIComponent"/> owning <see langword="this" /> text is not yet assigned to a window or a component with a rendering viewport.
    /// </returns>
    public Size ComputeTextSize(uint? index = null)
    {
        var text = DisplayText.Value;

        if (index is not null && index > text.Length)
        {
            throw new ArgumentOutOfRangeException($"Cannot calculate advance for index {index} of text {text}: index cannot be greater than the text size ({text.Length})");
        }

        if (index is not null)
        {
            text = text.Substring(0, (int)index);
        }


        return RenderingEngine?.ComputeTextSize(text, Font.Value.ToGraphicsFont()) ?? Size.Zero;
    }

    /// <summary>
    /// Computes each character width separately for the given <paramref name="text"/>.
    /// </summary>
    /// <param name="text">The text for which to compute widths.</param>
    /// <param name="font">The font the text is rendered.</param>
    /// <returns>
    /// An array of widths <c>W</c> where <c>W[i]</c> is the total width occupide by
    /// <paramref name="text"/><c>[i]</c> including advance.
    /// </returns>
    public PixelUnit[] ComputeCharactersWidths(string text, Font font)
    {
        if (RenderingEngine == null) throw new NullReferenceException("Cannot compute characters width: no rendering engine is defined");
        return RenderingEngine.ComputeCharactersWidths(text, font.ToGraphicsFont());
    }

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
        _text.ForegroundColor = TextColor.Value;
        _text.Value = DisplayText.Value;
        _text.Font = Font.Value.ToGraphicsFont();
        var textSize = ComputeTextSize();
        var x = TextHorizontalAlignment.Value.ComputeX(RelativeRenderingArea.Value.Size, textSize) + HorizontalScroll.Value;
        var y = TextVerticalAlignment.Value.ComputeY(RelativeRenderingArea.Value.Size, textSize);
        var area = new Area(new Point(x, y), textSize);
        var parentRelativeArea = area.ToAbsolute(RelativeRenderingArea.Value);
        _text.RenderingArea = parentRelativeArea.ToAbsolute(Canvas?.ContainerAbsoluteDrawingArea);
    }
}
