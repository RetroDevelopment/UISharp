using static SDL2.SDL;

namespace RetroDev.OpenUI.Core.Windowing.SDL;

/// <summary>
/// Idenrifies windows created with SDL.
/// </summary>
/// <param name="id">The SDL window identifier.</param>
public class SDLWindowId : IWindowId
{
    public uint Id { get; }
    public nint Handle { get; }

    public SDLWindowId(uint id)
    {
        Id = id;
        Handle = SDL_GetWindowFromID(id);
    }

    public SDLWindowId(nint handle)
    {
        Id = SDL_GetWindowID(handle);
        Handle = handle;
    }

    // IEquatable implementation
    public bool Equals(IWindowId? other)
    {
        if (other == null) return false;
        if (other is SDLWindowId windowId)
        {
            return Id == windowId.Id;
        }

        return false;
    }

    // Override GetHashCode to use Id
    public override int GetHashCode() => Id.GetHashCode();

    // Override Equals for object equality
    public override bool Equals(object? obj) => Equals(obj as IWindowId);

    /// <inheritdoc />
    public override string ToString() => Id.ToString();
}
