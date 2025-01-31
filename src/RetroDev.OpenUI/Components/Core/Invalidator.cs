using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;

namespace RetroDev.OpenUI.Components.Core;

internal class Invalidator
{
    private Dictionary<int, HashSet<UIComponent>> _invalidatedItems = [];

    public int TreeDepth => _invalidatedItems.Keys.Max() + 1;

    public void Invalidate(UIComponent component)
    {
        var level = component._level;
        _invalidatedItems.TryAdd(level, []);
        _invalidatedItems[level].Add(component);
    }

    public void CancelInvalidation(UIComponent component)
    {
        var level = component._level;
        _invalidatedItems[level].Remove(component);
        if (_invalidatedItems[level].Count == 0) _invalidatedItems.Remove(level);
    }

    public int GetNextInvalidatedLevel(int level)
    {
        var keys = _invalidatedItems.Keys.ToList();
        var index = keys.FindIndex(k => k == level);
        return index != 0 ? keys[index - 1] : -1;
    }

    public void AddInvalidatedComponentsToQueue(int level, UniqueQueue<UIComponent> queue)
    {
        if (!_invalidatedItems.ContainsKey(level)) return;

        foreach (var element in _invalidatedItems[level])
        {
            queue.Enqueue(element);
        }
    }

    // TODO: do we need it?
    public void Clear() { _invalidatedItems.Clear(); }
}
