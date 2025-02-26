﻿using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Components.Base;

namespace RetroDev.OpenUI.Components.Core;

public class Invalidator
{
    private SortedDictionary<int, HashSet<UIComponent>> _invalidatedItems = [];

    internal bool NeedZIndexUpdate { get; set; } = true;
    public int TreeDepth => _invalidatedItems.Keys.LastOrDefault(-1) + 1;

    public void Invalidate(UIComponent component)
    {
        var level = component._level;
        _invalidatedItems.TryAdd(level, []);
        _invalidatedItems[level].Add(component);
    }

    public void CancelInvalidation(UIComponent component)
    {
        var level = component._level;
        // No need to cancel invalidation if the componet has not been invalidated
        if (!_invalidatedItems.ContainsKey(level) || !_invalidatedItems[level].Contains(component)) return;
        _invalidatedItems[level].Remove(component);
        if (_invalidatedItems[level].Count == 0)
        {
            _invalidatedItems.Remove(level);
        }
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
    public void Reset()
    {
        _invalidatedItems.Clear();
    }
}
