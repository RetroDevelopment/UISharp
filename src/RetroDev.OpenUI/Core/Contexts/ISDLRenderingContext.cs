using RetroDev.OpenUI.Core.Windowing.SDL;

namespace RetroDev.OpenUI.Core.Contexts;

/// <summary>
/// The SDL rendering context. It allows creating windows using SDL that match a specific <see cref="IRenderingContext"/> implementation.
/// </summary>
public interface ISDLRenderingContext
{
    /// <summary>
    /// The identiier of the window created by <see langword="this" /> <see cref="IRenderingContext"/>.
    /// </summary>
    public SDLWindowId WindowId { get; }
}
