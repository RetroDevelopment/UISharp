using RetroDev.UISharp.Core.Windowing;

namespace RetroDev.UISharp.Core.Graphics.Shapes;

/// <summary>
/// A circle.
/// </summary>
/// <param name="dispatcher">Manages the UI thread and dispatches UI operations from other thread to the UI thread.</param>
public class Circle(ThreadDispatcher dispatcher) : Shape(dispatcher)
{
}
