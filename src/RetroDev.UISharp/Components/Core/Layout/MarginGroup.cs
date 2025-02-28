using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Core.Graphics.Coordinates;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Layout;

// TODO: marings in UIDefinition

/// <summary>
/// Defines margins for a <see cref="UIComponent"/>.
/// </summary>
public class MarginGroup<TComponent> where TComponent : UIComponent, new()
{
    public UIProperty<TComponent, PixelUnit> Top { get; }
    public UIProperty<TComponent, PixelUnit> Right { get; }
    public UIProperty<TComponent, PixelUnit> Bottom { get; }
    public UIProperty<TComponent, PixelUnit> Left { get; }

    public MarginGroup(UIComponent parent)
    {

    }
}
