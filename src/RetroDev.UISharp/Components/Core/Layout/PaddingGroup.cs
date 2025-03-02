using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Layout;

/// <summary>
/// Contains padding information.
/// </summary>
public class PaddingGroup : CompositeBindableProperty<PixelUnit>
{
    /// <summary>
    /// Top padding.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Top { get; }

    /// <summary>
    /// Right padding.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Right { get; }

    /// <summary>
    /// Bottom padding.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Bottom { get; }

    /// <summary>
    /// Left padding.
    /// </summary>
    public UIProperty<UIComponent, PixelUnit> Left { get; }

    /// <summary>
    /// Creates a new <see cref="PaddingGroup"/>.
    /// </summary>
    /// <param name="application">The application in which the given <paramref name="parent"/> component is running.</param>
    /// <param name="parent">The component owning <see langword="this" /> <see cref="PaddingGroup"/>.</param>
    public PaddingGroup(Application application, UIComponent parent) : base(application)
    {
        Top = new UIProperty<UIComponent, PixelUnit>(parent, PixelUnit.Auto);
        Right = new UIProperty<UIComponent, PixelUnit>(parent, PixelUnit.Auto);
        Bottom = new UIProperty<UIComponent, PixelUnit>(parent, PixelUnit.Auto);
        Left = new UIProperty<UIComponent, PixelUnit>(parent, PixelUnit.Auto);
    }

    /// <summary>
    /// Converts this <see cref="PaddingGroup"/> into a equivalent <see cref="Margin"/> instance.
    /// </summary>
    public Margin ToMarginStruct() => new(Top.Value, Right.Value, Bottom.Value, Left.Value);
}
