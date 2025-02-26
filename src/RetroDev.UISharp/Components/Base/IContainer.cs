namespace RetroDev.UISharp.Components.Base;

/// <summary>
/// All containers containing multiple child must implement this interface.
/// </summary>
public interface IContainer : IGenericContainer
{
    /// <summary>
    /// Adds a component to <see langword="this" /> container.
    /// </summary>
    /// <param name="component">The component to add.</param>
    void AddComponent(UIWidget component);
}
