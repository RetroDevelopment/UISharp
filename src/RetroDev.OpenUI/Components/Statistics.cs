using System.Diagnostics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components;

/// <summary>
/// Provides statistics for performance tracking and debugging.
/// </summary>
public class Statistics<TComponent> where TComponent : UIComponent
{
    private Stopwatch _stopwatch = new Stopwatch();

    public BindableProperty<TComponent, string> ShapesLastFrame { get; }

    /// <summary>
    /// Creates a new statisics.
    /// </summary>
    /// <param name="parent">The parent that generates statistics (usually a <see cref="Window"/>).</param>
    public Statistics(TComponent parent)
    {
        ShapesLastFrame = new UIProperty<TComponent, string>(parent, "-");
        _stopwatch.Start();
    }
}
