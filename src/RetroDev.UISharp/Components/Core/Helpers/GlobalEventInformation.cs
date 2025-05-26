using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.Components.Core.Helpers;

internal class GlobalEventInformation
{
    private List<UIObject> _draggingComponents = [];
    public IEnumerable<UIObject> DraggingComponents => _draggingComponents;

    public void MarkComponentAsDragged(UIObject component)
    {
        _draggingComponents.Add(component);
    }

    public void ClearDraggedComponents()
    {
        _draggingComponents.Clear();
    }
}
