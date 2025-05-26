namespace RetroDev.UISharp.Core.Graphics.Fonts;

/// <summary>
/// A text font.
/// </summary>
public readonly struct Font
{
    /// <summary>
    /// A uinique font identifier.
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// The font size in PT.
    /// </summary>
    public float Size { get; }

    /// <summary>
    /// The font file content.
    /// </summary>
    internal byte[] Data { get; }

    /// <summary>
    /// Creates a new font.
    /// </summary>
    /// <param name="data">The font file binary data.</param>
    /// <param name="identifier">A uinque font identifier used for caching purposes.</param>
    /// <param name="size">Font size in PT.</param>
    public Font(byte[] data, string identifier, float size)
    {
        Data = data;
        Identifier = identifier;
        Size = size;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Exclude Data from hash code calculation
        var hashCode = Identifier.GetHashCode();
        hashCode = (hashCode * 397) ^ Size.GetHashCode();
        return hashCode;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        // Exclude Data from equality calculation
        if (obj is Font other)
        {
            return Identifier == other.Identifier && Size == other.Size;
        }
        return false;
    }
}
