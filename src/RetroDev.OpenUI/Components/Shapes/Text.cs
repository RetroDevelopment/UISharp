using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.UI;
using RetroDev.OpenUI.UI.Coordinates;
using RetroDev.OpenUI.UI.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// Displays test in the UI.
/// </summary>
public class Text : UIWidget
{
    /// <summary>
    /// The text color.
    /// </summary>
    public UIProperty<Text, Color> TextColor { get; }

    /// <summary>
    /// The text to display.
    /// </summary>
    public UIProperty<Text, string> DisplayText { get; }

    /// <summary>
    /// The <see cref="DisplayText"/> <see cref="Font"/>.
    /// </summary>
    public UIProperty<Text, Font> Font { get; }

    /// <summary>
    /// Creates a new text.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Text(Application application) : base(application, isFocusable: false)
    {
        TextColor = new UIProperty<Text, Color>(this, Color.Transparent);
        DisplayText = new UIProperty<Text, string>(this, string.Empty);
        Font = new UIProperty<Text, Font>(this, new Font(application, "OpenSans", 20, FontType.Regular));
        RenderFrame += Rectangle_RenderFrame;
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        if (Root == null) return Size.Zero;
        return Root.RenderingEngine.ComputeTextSize(DisplayText.Value, Font.Value.ToGraphicsFont());
    }

    private void Rectangle_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var textShape = new OpenUI.Core.Graphics.Shapes.Text(BackgroundColor.Value,
                                                             TextColor.Value,
                                                             DisplayText.Value,
                                                             Font.Value.ToGraphicsFont());
        var canvas = e.Canvas;

        canvas.Render(textShape, ActualSize.Fill());
    }
}
