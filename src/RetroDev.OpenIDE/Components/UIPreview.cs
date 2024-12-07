using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;

namespace RetroDev.OpenIDE.Components;

[EditorSettings(allow: false)]
internal class UIPreview : UIComponent
{
    public UIPreview(Application application, List<UIComponent> children) : base(application)
    {
        children.ForEach(c => AddChild(c));
        AutoWidth.Value = AutoSizeStrategy.MatchParent;
        AutoHeight.Value = AutoSizeStrategy.MatchParent;
    }

    protected override Size ComputeSizeHint() => new(100, 100);
}
