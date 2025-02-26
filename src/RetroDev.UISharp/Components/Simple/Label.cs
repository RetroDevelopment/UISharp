using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Coordinates;
using RetroDev.UISharp.Presentation;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A label displaying text.
/// </summary>
public class Label : UIWidget
{
    private readonly Text _text;

    /// <summary>
    /// The display text.
    /// </summary>
    public UIProperty<Label, string> Text { get; }

    /// <summary>
    /// The text color.
    /// </summary>
    public UIProperty<Label, Color> TextColor { get; }

    /// <summary>
    /// The label font.
    /// </summary>
    public UIProperty<Label, Font> Font { get; }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => _text.ComputeTextSize();

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="application">The application that contain this label.</param>
    public Label(Application application) : base(application, isFocusable: false, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Text = new UIProperty<Label, string>(this, string.Empty);
        Font = new UIProperty<Label, Font>(this, Application.DefaultFont, BindingType.DestinationToSource);
        TextColor = new UIProperty<Label, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);

        _text = new Text(application);
        _text.BackgroundColor.BindDestinationToSource(BackgroundColor);
        _text.TextColor.BindDestinationToSource(TextColor);
        _text.DisplayText.BindDestinationToSource(Text);
        _text.Font.BindDestinationToSource(Font);

        RenderFrame += Label_RenderFrame;
        Canvas.Add(_text);
    }

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="parent">The application that contain this label.</param>
    /// <param name="text">The label text.</param>
    public Label(Application parent, string text) : this(parent)
    {
        Text.Value = text;
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

    private void Label_RenderFrame(UIComponent sender, UISharp.Core.Windowing.Events.RenderingEventArgs e)
    {
        _text.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
    }
}
