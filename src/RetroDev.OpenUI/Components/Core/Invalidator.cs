using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Components.Base;

namespace RetroDev.OpenUI.Components.Core;

public class Invalidator
{
    private readonly SortedDictionary<int, HashSet<UIComponent>> _firstPassInvalidatedItems = [];
    private readonly SortedDictionary<int, HashSet<UIComponent>> _secondPassInvalidatedItems = [];
    private SortedDictionary<int, HashSet<UIComponent>> _invalidatedItems;

    internal bool NeedZIndexUpdate { get; set; } = true;
    public int TreeDepth => _invalidatedItems.Keys.LastOrDefault(-1) + 1;

    public Invalidator()
    {
        _invalidatedItems = _firstPassInvalidatedItems;
    }

    public void Invalidate(UIComponent component)
    {
        Invalidate(component, _firstPassInvalidatedItems);
        Invalidate(component, _secondPassInvalidatedItems);
    }

    public void CancelInvalidation(UIComponent component)
    {
        CancelInvalidation(component, _firstPassInvalidatedItems);
        CancelInvalidation(component, _secondPassInvalidatedItems);
    }

    public int GetUpperInvalidatedLevel(int level) =>
        _invalidatedItems.Keys.Reverse().FirstOrDefault(k => k < level, -1);

    public int GetLowerInvalidatedLevel(int level) =>
        _invalidatedItems.Keys.FirstOrDefault(k => k > level, -1);

    public UIComponent? GetNextInvalidatedComponent()
    {
        if (_invalidatedItems.Count == 0) return null;

        var topMostLevel = _invalidatedItems.Keys.First();
        return _invalidatedItems[topMostLevel].First();
    }

    public void AddInvalidatedComponentsToQueue(int level, UniqueQueue<UIComponent> queue)
    {
        if (!_invalidatedItems.ContainsKey(level)) return;

        foreach (var element in _invalidatedItems[level])
        {
            queue.Enqueue(element);
        }
    }

    // TODO: remove this once implementing instancing
    public void Reset(bool secondPass = false)
    {
        _secondPassInvalidatedItems.Clear();
        if (!secondPass) _firstPassInvalidatedItems.Clear();
    }

    public void Swap()
    {
        if (_invalidatedItems == _firstPassInvalidatedItems)
        {
            _invalidatedItems = _secondPassInvalidatedItems;
        }
        else
        {
            _invalidatedItems = _firstPassInvalidatedItems;
        }
    }

    private void Invalidate(UIComponent component, SortedDictionary<int, HashSet<UIComponent>> invalidatedItems)
    {
        var level = component._level;
        invalidatedItems.TryAdd(level, []);
        invalidatedItems[level].Add(component);
    }

    private void CancelInvalidation(UIComponent component, SortedDictionary<int, HashSet<UIComponent>> invalidatedItems)
    {
        var level = component._level;
        // No need to cancel invalidation if the componet has not been invalidated
        if (!invalidatedItems.ContainsKey(level) || !invalidatedItems[level].Contains(component)) return;
        invalidatedItems[level].Remove(component);
        if (invalidatedItems[level].Count == 0)
        {
            invalidatedItems.Remove(level);
        }
    }
}
