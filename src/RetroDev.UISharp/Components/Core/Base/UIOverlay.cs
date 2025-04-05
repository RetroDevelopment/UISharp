namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// Wraps a <see cref="UIControl"/> that is an overlay, that is, it is rendered on the top of the
/// <see cref="UIControl"/> in the UI hierarchy.
/// </summary>
public abstract class UIOverlay
{
    /// <summary>
    /// The application owning this overlay.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The actual <see cref="UIControl"/> to display for <see langword="this" /> <see cref="UIOverlay"/>.
    /// </summary>
    public UIControl Control { get; }

    /// <summary>
    /// Creates a new UI overlay.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="control">The wrapped <see cref="UIControl"/>.</param>
    protected UIOverlay(Application application, UIControl control)
    {
        Application = application;
        Control = control;
    }

    /// <summary>
    /// Attaches the owner.
    /// </summary>
    /// <param name="owner">A <see cref="UIObject"/> for which to display <see langword="this" /> <see cref="UIOverlay"/>.</param>
    protected internal abstract void AttachOwner(UIObject owner);

    /// <summary>
    /// Detaches the owner.
    /// </summary>
    /// <param name="owner">A <see cref="UIObject"/> for which to display <see langword="this" /> <see cref="UIOverlay"/>.</param>
    protected internal abstract void DetachOwner(UIObject owner);
}
