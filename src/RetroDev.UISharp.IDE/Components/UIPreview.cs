using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.IDE.Components;

[EditorSettings(allow: false)]
internal class UIPreview : UIContainer
{
    public UIPreview(Application application, List<UIWidget> children) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        children.ForEach(c => AddChildNode(c));
    }

    public override IEnumerable<UIWidget> Children => base.GetChildrenNodes();

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

            var childX = child.X.Value.IsAuto ? PixelUnit.Zero : child.X.Value;
            var childY = child.Y.Value.IsAuto ? PixelUnit.Zero : child.Y.Value;
            var childWidth = childWrapSize.Width;
            var childHeight = childWrapSize.Height;
            maxRight = Math.Max(maxRight, childX + childWidth);
            maxBottom = Math.Max(maxBottom, childY + childHeight);
        }

        return new Size(maxRight, maxBottom);
    }
}
