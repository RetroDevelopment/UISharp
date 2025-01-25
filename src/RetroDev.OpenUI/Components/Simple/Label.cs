using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A label displaying text.
/// </summary>
public class Label : UIComponent
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

    /// <inheritdoc/>
    protected override Size ComputeSizeHint() =>
        Application.FontServices.ComputeTextSize(Text.Value);

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="application">The application that contain this label.</param>
    public Label(Application application) : base(application, isFocusable: false, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Text = new UIProperty<Label, string>(this, string.Empty);
        TextColor = new UIProperty<Label, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);

        _text = new Text(application);
        _text.BackgroundColor.BindDestinationToSource(BackgroundColor);
        _text.TextColor.BindDestinationToSource(TextColor);
        _text.DisplayText.BindDestinationToSource(Text);
        AddChild(_text);
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
