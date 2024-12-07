using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenUI.Components.AutoSize;

public class WrapComponentLeftTopAlignStrategy : IAutoSizeStrategy
{
    public (PixelUnit X, PixelUnit Width) ComputeHorizontalArea(UIComponent component)
    {
        var width = component.Width.Value.IsAuto ? component.SizeHint.Width : component.Width;
        return (component.X.Value.IsAuto ? 0.0f : component.X, width);
    }

    public (PixelUnit Y, PixelUnit Height) ComputeVerticalArea(UIComponent component)
    {
        var height = component.Height.Value.IsAuto ? component.SizeHint.Height : component.Height;
        return (component.Y.Value.IsAuto ? 0.0f : component.Y, height);
    }
}
