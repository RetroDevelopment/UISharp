namespace RetroDev.OpenUI.Components.Base;

/// <summary>
/// Represents a container.
/// </summary>
public interface IGenericContainer
{
    /// <summary>
    /// The child <see cref="UIComponent"/> contained by <see langword="this" /> container.
    /// </summary>
    public IEnumerable<UIWidget> Children { get; }
}
