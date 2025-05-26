namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// The common interface for all the UI properties.
/// </summary>
public interface IProperty
{
    /// <summary>
    /// Remove bindings from <see langword="this" /> property.
    /// </summary>
    void Unbind();
}
