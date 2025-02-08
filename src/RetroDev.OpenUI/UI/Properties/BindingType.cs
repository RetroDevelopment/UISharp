namespace RetroDev.OpenUI.UI.Properties;

/// <summary>
/// Specifies how a binding occurs between a source property to a destination binder.
/// </summary>
public enum BindingType
{
    /// <summary>
    /// Source and destination are kept in sync. This is a combination of
    /// <see cref="SourceToDestination"/> and <see cref="DestinationToSource"/>.
    /// </summary>
    TwoWays,

    /// <summary>
    /// Whenever source is updated, destination is also updated.
    /// Whenever destination is updated, source is NOT updated.
    /// </summary>
    SourceToDestination,

    /// <summary>
    /// Whenever destination is updated, source is also updated.
    /// Whenever source is updated, destination is NOT updated.
    /// </summary>
    DestinationToSource,
}
