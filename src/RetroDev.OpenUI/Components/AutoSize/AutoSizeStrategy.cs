namespace RetroDev.OpenUI.Components.AutoSize;

public static class AutoSizeStrategy
{
    public static readonly IAutoSizeStrategy MatchParent = new MatchParentSizeStrategy();
    public static readonly IAutoSizeStrategy WrapComponentLeftTop = new WrapComponentLeftTopAlignStrategy();
    public static readonly IAutoSizeStrategy WrapComponentCenter = new WrapContenCenterAlignStrategy();
    public static readonly IAutoSizeStrategy WrapComponentRightBottom = new WrapContenRightBottomAlignStrategy();
}
