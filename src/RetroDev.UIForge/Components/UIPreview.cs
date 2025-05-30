﻿using RetroDev.UISharp.Components.Core;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.IDE.Components;

[EditorSettings(allow: false)]
internal class UIPreview : UIContainer
{
    public UIPreview(Application application, List<UIControl> children) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        children.ForEach(Children.Add);
    }

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        var children = Children.ToList();
        var numberOfChildren = children.Count;
        var childrenSizeList = childrenSize.ToList();
        var maxRight = PixelUnit.Zero;
        var maxBottom = PixelUnit.Zero;

        for (int i = 0; i < numberOfChildren; i++)
        {
            var child = children[i];
            var childWrapSize = childrenSizeList[i];

            var childX = child.X.Value.IfAuto(PixelUnit.Zero);
            var childY = child.Y.Value.IfAuto(PixelUnit.Zero);
            var childWidth = childWrapSize.Width;
            var childHeight = childWrapSize.Height;
            maxRight = Math.Max(maxRight, childX + childWidth);
            maxBottom = Math.Max(maxBottom, childY + childHeight);
        }

        return new Size(maxRight, maxBottom);
    }
}
