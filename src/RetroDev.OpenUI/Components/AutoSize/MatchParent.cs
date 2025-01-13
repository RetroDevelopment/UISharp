using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoSize;

// TODO: default width/height for orphans (e.g. windows) is now 800 x 600. This will need to be set to screen size? Or
// some other default size? Orphans are not only windows but also compoennts not yet attached to a window.
public class MatchParent : IAutoSizeStrategy
{
    private static readonly PixelUnit DefaultWidth = 800.0f;
    private static readonly PixelUnit DefaultHeight = 600.0f;

    public (PixelUnit X, PixelUnit Width) ComputeHorizontalArea(UIComponent component)
    {
        var autoWidth = component.Parent?.RelativeDrawingArea?.Size?.Width ?? DefaultWidth;

        return (component.X.Value.IsAuto ? 0.0f : component.X,
                component.Width.Value.IsAuto ? autoWidth : component.Width.Value);
    }

    public (PixelUnit Y, PixelUnit Height) ComputeVerticalArea(UIComponent component)
    {
        var autoHeight = component.Parent?.RelativeDrawingArea?.Size?.Height ?? DefaultHeight;

        return (component.Y.Value.IsAuto ? 0.0f : component.Y,
                component.Height.Value.IsAuto ? autoHeight : component.Height.Value);
    }
}
