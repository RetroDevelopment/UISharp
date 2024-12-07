using System.ComponentModel;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoSize;

public class WrapContenCenterAlignStrategy : IAutoSizeStrategy
{
    public (PixelUnit X, PixelUnit Width) ComputeHorizontalArea(UIComponent component)
    {
        var width = component.Width.Value.IsAuto ? component.SizeHint.Width : component.Width;
        var center = ((component.Parent?.RelativeDrawingArea?.Size?.Width?.Value ?? 800.0f) / 2.0f) - width / 2; // TODO: use screen area size as default widht?
        return (component.X.Value.IsAuto ? center : component.X, width);
    }

    public (PixelUnit Y, PixelUnit Height) ComputeVerticalArea(UIComponent component)
    {
        var height = component.Height.Value.IsAuto ? component.SizeHint.Height : component.Height;
        var center = ((component.Parent?.RelativeDrawingArea?.Size?.Height?.Value ?? 600.0f) / 2.0f) - height / 2; // TODO: use screen area size as default widht?
        return (component.Y.Value.IsAuto ? center : component.Y, height);
    }
}
