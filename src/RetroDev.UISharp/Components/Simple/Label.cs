using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A label displaying text.
/// </summary>
public class Label : UIWidget
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Text _text;

    /// <summary>
    /// The display text.
    /// </summary>
    public UIProperty<string> Text { get; }

    /// <summary>
    /// The text color.
    /// </summary>
    public UIProperty<Color> TextColor { get; }

    /// <summary>
    /// The label font.
    /// </summary>
    public UIProperty<Font> Font { get; }

    /// <summary>
    /// The text horizontal alignment within the label.
    /// </summary>
    public UIProperty<IHorizontalAlignment> TextHorizontalAlignment { get; }

    /// <summary>
    /// The text vertical alignment within the label.
    /// </summary>
    public UIProperty<IVerticalAlignment> TextVerticalAlignment { get; }

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="application">The application that contain this label.</param>
    /// <param name="text">The label display text.</param>
    public Label(Application application, string text = "") : base(application, isFocusable: false, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Text = new UIProperty<string>(this, text);
        Font = new UIProperty<Font>(this, Application.DefaultFont, BindingType.SourceToDestination);
        TextColor = CreateNewColorPropertyFor<Label>(UISharpColorNames.LabelText);
        TextHorizontalAlignment = new UIProperty<IHorizontalAlignment>(this, Alignment.Center);
        TextVerticalAlignment = new UIProperty<IVerticalAlignment>(this, Alignment.Center);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindSourceToDestination(BackgroundColor);
        Canvas.Add(_backgroundRectangle);

        _text = new Text(application);
        _text.TextColor.BindSourceToDestination(TextColor);
        _text.DisplayText.BindSourceToDestination(Text);
        _text.Font.BindSourceToDestination(Font);
        _text.TextHorizontalAlignment.BindSourceToDestination(TextHorizontalAlignment);
        _text.TextVerticalAlignment.BindSourceToDestination(TextVerticalAlignment);
        Canvas.Add(_text);

        RenderFrame += Label_RenderFrame;
    }

    /// <summary>
    /// Computes the minimum size needed to display exactly the full text.
    /// </summary>
    /// <returns>The minimum size to display <see langword="this" /> <see cref="Label"/> <see cref="Text"/>.</returns>
    public Size ComputeTextSize() => _text.ComputeTextSize();

    /// <summary>
    /// Computes the height necessary to display any character of text using the given <see cref="Font"/>.
    /// </summary>
    /// <returns>The height necessary to display any character of text using the given <see cref="Font"/>.</returns>
    public PixelUnit ComputeTextMaximumHeight() => _text.ComputeTextMaximumHeight();

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => _text.ComputeTextSize();

    private void Label_RenderFrame(UIComponent sender, UISharp.Core.Windowing.Events.RenderingEventArgs e)
    {
        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();

        var textSize = ComputeTextSize();
        var parentSize = e.RenderingAreaSize;
        var x = TextHorizontalAlignment.Value.ComputeX(parentSize, textSize);
        var y = TextVerticalAlignment.Value.ComputeY(parentSize, textSize);
        var textArea = new Area(new Point(x, y), textSize).Clamp(parentSize, Padding.ToMarginStruct());
        _text.RelativeRenderingArea.Value = textArea;
        _text.ClipArea.Value = textArea;
    }
}
