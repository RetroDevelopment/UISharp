using RetroDev.OpenUI.Components.Base;

namespace RetroDev.OpenUI.Components;

/// <summary>
/// Defines <see cref="UIComponent"/> visibility rules.
/// </summary>
public enum ComponentVisibility
{
    /// <summary>
    /// The component is visible.
    /// </summary>
    Visible,

    /// <summary>
    /// The component is invisible but it still occupies the space in the UI, meaning that it preserves the <see cref="UIComponent.RelativeDrawingArea"/> and
    /// <see cref="UIComponent.AbsoluteDrawingArea"/>.
    /// </summary>
    Hidden,

    /// <summary>
    /// The component is invisible and it does not occupy any space, meaning that the size is zero in both <see cref="UIComponent.RelativeDrawingArea"/> and
    /// <see cref="UIComponent.AbsoluteDrawingArea"/>.
    /// </summary>
    Collapsed
}
