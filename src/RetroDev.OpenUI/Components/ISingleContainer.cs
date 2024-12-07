namespace RetroDev.OpenUI.Components;

/// <summary>
/// All containers implementing a single child must implement this interface.
/// </summary>
public interface ISingleContainer : IGenericContainer
{
    /// <summary>
    /// Sets the child component.
    /// </summary>
    /// <param name="component">The child component contained in <see langword="this" /> container.</param>
    public void SetComponent(UIComponent component);
}
