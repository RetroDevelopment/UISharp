namespace RetroDev.UISharp.Presentation.Properties.Binding;

/// <summary>
/// Specifies how a binding occurs between a source property to a destination binder.
/// In a binding in the form <c>p1.Bind(p2)</c>, the <c>p2</c> property is considered the <c>source</c>
/// and <c>p1</c> is the <c>destination</c>.
/// </summary>
public enum BindingType
{
    /// <summary>
    /// Source and destination are kept in sync. This is a combination of
    /// <see cref="SourceToDestination"/> and <see cref="DestinationToSource"/>.
    /// </summary>
    TwoWays,

    /// <summary>
    /// Whenever destination is updated, source is also updated.
    /// Whenever source is updated, destination is NOT updated.
    /// </summary>
    SourceToDestination,

    /// <summary>
    /// Whenever source is updated, destination is also updated.
    /// Whenever destination is updated, source is NOT updated.
    /// </summary>
    DestinationToSource,
}
