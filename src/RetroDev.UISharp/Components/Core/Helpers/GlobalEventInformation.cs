using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.Components.Core.Helpers;

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
