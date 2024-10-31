namespace RetroDev.OpenUI.Core.Internal;

internal class SDLWindowId(int id) : IWindowId
{
    public int Id { get; private init; } = id;

    // IEquatable implementation
    public bool Equals(IWindowId? other)
    {
        if (other == null) return false;
        return Id == other.Id;
    }

    // Override GetHashCode to use Id
    public override int GetHashCode() => Id.GetHashCode();

    // Override Equals for object equality
    public override bool Equals(object? obj) => Equals(obj as IWindowId);
}
