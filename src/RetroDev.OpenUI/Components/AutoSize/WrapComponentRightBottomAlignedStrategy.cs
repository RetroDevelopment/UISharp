using System.ComponentModel;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoSize;

public class WrapContenRightBottomAlignStrategy : IAutoSizeStrategy
{
    public (PixelUnit X, PixelUnit Width) ComputeHorizontalArea(UIComponent component)
    {
        var width = component.Width.Value.IsAuto ? component.SizeHint.Width : component.Width;
        var right = (component.Parent?.Width.Value ?? 800.0f) - width; // TODO: use screen area size as default widht?
        return (component.X.Value.IsAuto ? right : component.X, width);
    }

    public (PixelUnit Y, PixelUnit Height) ComputeVerticalArea(UIComponent component)
    {
        var height = component.Height.Value.IsAuto ? component.SizeHint.Height : component.Height;
        var bottom = (component.Parent?.Height.Value ?? 600.0f) - height; // TODO: use screen area size as default widht?
        return (component.Y.Value.IsAuto ? bottom : component.Y, height);
    }
}
