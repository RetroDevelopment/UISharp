namespace RetroDev.OpenUI.Components.Core;

/// <summary>
/// Class that performs optimized measurement of <see cref="UIComponent"/> rendering areas, meaning that 
/// where only invalidated elements will be measured.
/// </summary>
/// <param name="invalidator"></param>
public class MeasureProvider(Invalidator invalidator)
{
    private readonly Invalidator _invalidator = invalidator;

    /// <summary>
    /// Measures component sizes before rendering.
    /// </summary>
    public void Measure()
    {
        RecomputeWrapSizes();
        RecomputeDrawingAreas();
    }

    /// <summary>
    /// Re-computes wrap sizes of invalidated elements.
    /// </summary>
    public void RecomputeWrapSizes()
    {
        if (_invalidator.TreeDepth == 0) return;

        var level = _invalidator.TreeDepth - 1;
        var processQueue = new UniqueQueue<UIComponent>();
        _invalidator.AddInvalidatedComponentsToQueue(level, processQueue);

        while (level >= 0)
        {
            var component = processQueue.Dequeue();
            var wrapSizeChanged = component.ReComputeWrapSize();

            if (wrapSizeChanged)
            {
                // TODO: invalidate only if wrap size actually affects the component rendering area (e.g. manual size or autoWidth or height to wrap)?
                // Or maybe optimize RecomputeRenderingArea() so that it doesn't visit all children if no change is made to rendering area?
                component.Invalidate();
                if (component.Parent != null) processQueue.Enqueue(component.Parent);
            }

            if (ShouldChangeLevel(processQueue, level))
            {
                level = ChangeLevel(processQueue, level);
            }
        }
    }

    /// <summary>
    /// Re-computes components drawing areas of invalidated elements.
    /// </summary>
    public void RecomputeDrawingAreas()
    {
        var level = 0;
        var processQueue = new UniqueQueue<UIComponent>();

        while (level != -1)
        {
            _invalidator.AddInvalidatedComponentsToQueue(level, processQueue);

            while (!processQueue.Empty)
            {
                var component = processQueue.Dequeue();
                component.ComputeDrawingAreas(rootCall: true);
            }

            level = _invalidator.GetLowerInvalidatedLevel(level);
        }
    }

    private bool ShouldChangeLevel(UniqueQueue<UIComponent> processQueue, int level) =>
        processQueue.Empty || processQueue.Peek()._level != level;

    private int ChangeLevel(UniqueQueue<UIComponent> processQueue, int level)
    {
        if (level == 0) return -1;

        var newLevel = level;

        if (!processQueue.Empty)
        {
            newLevel = processQueue.Peek()._level;
        }
        else
        {
            newLevel = _invalidator.GetUpperInvalidatedLevel(level);
        }

        if (newLevel >= 0) _invalidator.AddInvalidatedComponentsToQueue(newLevel, processQueue);

        return newLevel;
    }
}
