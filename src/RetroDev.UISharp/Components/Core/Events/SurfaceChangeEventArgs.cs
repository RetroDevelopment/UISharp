using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.Components.Core.Events;

/// <summary>
/// The <see cref="UISurface"/> in which a <see cref="UIObject"/> is rendered changes.
/// </summary>
/// <param name="previousSurface">The <see cref="UISurface"/> in which the target <see cref="UIObject"/> was attached, <see langword="null" /> if none.</param>
/// <param name="currentSurface">The <see cref="UISurface"/> in which the target <see cref="UIObject"/> is currently attached, <see langword="null" /> if none.</param>
public class SurfaceChangeEventArgs(UISurface? previousSurface, UISurface? currentSurface) : EventArgs
{
    /// <summary>
    /// The <see cref="UISurface"/> in which the target <see cref="UIObject"/> was attached, <see langword="null" /> if none.
    /// </summary>
    public UISurface? PreviousSurface { get; } = previousSurface;

    /// <summary>
    /// The <see cref="UISurface"/> in which the target <see cref="UIObject"/> is currently attached, <see langword="null" /> if none.
    /// </summary>
    public UISurface? CurrentSurface { get; } = currentSurface;
    public override string ToString() =>
        $"previousSurface: {PreviousSurface}, currentSurface: {CurrentSurface}";
}
