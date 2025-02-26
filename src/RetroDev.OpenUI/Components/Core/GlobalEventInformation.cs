using RetroDev.OpenUI.Components.Base;

namespace RetroDev.OpenUI.Components.Core;

internal class GlobalEventInformation
{
    private List<UIComponent> _draggingComponents = [];
    public IEnumerable<UIComponent> DraggingComponents => _draggingComponents;

    public void MarkComponentAsDragged(UIComponent component)
    {
        _draggingComponents.Add(component);
    }

    public void ClearDraggedComponents()
    {
        _draggingComponents.Clear();
    }
}
