﻿using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Layout;

/// <summary>
/// Contains margin information.
/// </summary>
public class MarginGroup : UICompositeProperty<PixelUnit>
{
    /// <summary>
    /// Top margin.
    /// </summary>
    public UIProperty<PixelUnit> Top { get; }

    /// <summary>
    /// Right margin.
    /// </summary>
    public UIProperty<PixelUnit> Right { get; }

    /// <summary>
    /// Bottom margin.
    /// </summary>
    public UIProperty<PixelUnit> Bottom { get; }

    /// <summary>
    /// Left margin.
    /// </summary>
    public UIProperty<PixelUnit> Left { get; }

    /// <summary>
    /// Creates a new <see cref="MarginGroup"/>.
    /// </summary>
    /// <param name="application">The application in which the given <paramref name="parent"/> component is running.</param>
    /// <param name="parent">The component owning <see langword="this" /> <see cref="MarginGroup"/>.</param>
    public MarginGroup(Application application, UIObject parent) : base(application)
    {
        Top = new UIProperty<PixelUnit>(parent, PixelUnit.Auto);
        Right = new UIProperty<PixelUnit>(parent, PixelUnit.Auto);
        Bottom = new UIProperty<PixelUnit>(parent, PixelUnit.Auto);
        Left = new UIProperty<PixelUnit>(parent, PixelUnit.Auto);
    }

    /// <summary>
    /// Converts this <see cref="MarginGroup"/> into a equivalent <see cref="Margin"/> instance.
    /// </summary>
    public Margin ToMarginStruct() =>
        new(Top.Value, Right.Value, Bottom.Value, Left.Value);
}
