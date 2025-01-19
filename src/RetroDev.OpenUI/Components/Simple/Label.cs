using RetroDev.OpenUI.Components.AutoArea;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;
using RetroDev.OpenUI.Properties;
using RetroDev.OpenUI.Themes;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A label displaying text.
/// </summary>
public class Label : UIComponent
{
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

    /// <inheritdoc />
    protected override bool DefaultIsFocusable => false;

    /// <inheritdoc />
    protected override IAutoSize DefaultAutoWidth => AutoSize.Wrap;

    /// <inheritdoc />
    protected override IAutoSize DefaultAutoHeight => AutoSize.Wrap;

    /// <inheritdoc />
    protected override IHorizontalAlignment DefaultHorizontalAlignment => Alignment.Center;

    /// <inheritdoc />
    protected override IVerticalAlignment DefaultVerticalAlignment => Alignment.Center;

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="application">The application that contain this label.</param>
    public Label(Application application) : base(application)
    {
        Text = new UIProperty<Label, string>(this, string.Empty);
        TextColor = new UIProperty<Label, Color>(this, Theme.DefaultColor);

        Text.ValueChange += (_, _) => SizeHintCache.MarkDirty();
        TextColor.Bind(Application.Theme.TextColor, BindingType.DestinationToSource);

        RenderFrame += Label_RenderFrame;
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

    private void Label_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var canvas = e.Canvas;
        var text = new Text(BackgroundColor, TextColor, Text);
        canvas.Render(text, RelativeDrawingArea.Fill());
    }
}
