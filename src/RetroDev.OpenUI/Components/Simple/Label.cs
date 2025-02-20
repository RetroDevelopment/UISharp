using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Presentation;
using RetroDev.OpenUI.Presentation.Properties;

namespace RetroDev.OpenUI.Components.Simple;

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
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.ElementAt(0);

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

        AddChildNode(_text);
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
}
