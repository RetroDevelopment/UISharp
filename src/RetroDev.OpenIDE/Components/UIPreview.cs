using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenIDE.Components;

[EditorSettings(allow: false)]
internal class UIPreview : Container
{
    public UIPreview(Application application, List<UIComponent> children) : base(application)
    {
        children.ForEach(c => AddChild(c));
    }

    public override IEnumerable<UIComponent> Children => throw new NotImplementedException();

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(100, 100);
}
