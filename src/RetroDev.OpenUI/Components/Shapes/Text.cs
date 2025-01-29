using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Shapes;

/// <summary>
/// Displays test in the UI.
/// </summary>
public class Text : UIComponent
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
    /// Creates a new text.
    /// </summary>
    /// <param name="application">The parent application.</param>
    public Text(Application application) : base(application, isFocusable: false)
    {
        TextColor = new UIProperty<Text, Color>(this, Color.Transparent);
        DisplayText = new UIProperty<Text, string>(this, string.Empty);

        RenderFrame += Rectangle_RenderFrame;
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        Application.FontServices.ComputeTextSize(DisplayText.Value);

    private void Rectangle_RenderFrame(UIComponent sender, Events.RenderingEventArgs e)
    {
        var rectangleShape = new Graphics.Shapes.Text(BackgroundColor.Value,
                                                      TextColor.Value,
                                                      DisplayText.Value);
        var canvas = e.Canvas;

        canvas.Render(rectangleShape, RelativeDrawingArea.Fill());
    }
}
