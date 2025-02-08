using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.UI.Coordinates;

namespace RetroDev.OpenIDE.Components;

[EditorSettings(allow: false)]
internal class UIPreview : Container
{
    public UIPreview(Application application, List<UIComponent> children) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        children.ForEach(c => AddChild(c));
    }

    public override IEnumerable<UIComponent> Children => base.GetChildren();

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
