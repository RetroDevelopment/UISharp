namespace RetroDev.UISharp.Presentation;

using RetroDev.UISharp.Presentation.Resources;

/// <summary>
/// The font type.
/// </summary>
public enum FontType
{
    /// <summary>
    /// Standard font.
    /// </summary>
    Regular,

    /// <summary>
    /// Blod font.
    /// </summary>
    Bold,

    /// <summary>
    /// Italic font (oblique looking).
    /// </summary>
    Italic,

    /// <summary>
    /// Both bold and italic font.
    /// </summary>
    BoldItalic
}

/// <summary>
/// A text font.
/// </summary>
public readonly struct Font
{
    /// <summary>
    /// The font name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The font size in PT.
    /// </summary>
    public float Size { get; }

    /// <summary>
    /// The font type.
    /// </summary>
    public FontType Type { get; }

    /// <summary>
    /// The font file content.
    /// </summary>
    internal byte[] Data { get; }

    /// <summary>
    /// Creates a new font.
    /// The font is loaded from <see cref="Application.ResourceManager"/> <see cref="IResourceManager.Font"/>.
    /// The expected font resource name is "<paramref name="name"/>-<paramref name="type"/>.ttf".
    /// </summary>
    /// <param name="application">The application from which to locate the font.</param>
    /// <param name="name">Font name.</param>
    /// <param name="size">Font size in PT.</param>
    /// <param name="type">Font type.</param>
    public Font(Application application, string name, float size, FontType type)
    {
        Name = name;
        Size = size;
        Type = type;
        Data = application.ResourceManager.Fonts[$"{name}-{type}"];
    }

    /// <summary>
    /// Creates a new font. Use this constructor overload if passing the font file data directly.
    /// </summary>
    /// <param name="data">The font file binary data.</param>
    /// <param name="name">Font name.</param>
    /// <param name="size">Font size in PT.</param>
    /// <param name="type">Font type.</param>
    public Font(byte[] data, string name, float size, FontType type)
    {
        Data = data;
        Name = name;
        Size = size;
        Type = type;
    }

    /// <summary>
    /// Creates a new <see cref="Font"/> with the same parameters as <see langword="this" />
    /// <see cref="Font"/> but with the given <paramref name="size"/>.
    /// </summary>
    /// <param name="size">The new font size.</param>
    /// <returns>A copy of <see langword="this" /> <see cref="Font"/> but with the given <paramref name="size"/>.</returns>
    public Font WithSize(float size) =>
        new(Data, Name, size, Type);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        // Exclude Data from hash code calculation
        var hashCode = Name.GetHashCode();
        hashCode = (hashCode * 397) ^ Size.GetHashCode();
        hashCode = (hashCode * 397) ^ Type.GetHashCode();
        return hashCode;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        // Exclude Data from equality calculation
        if (obj is Font other)
        {
            return Name == other.Name && Size == other.Size && Type == other.Type;
        }
        return false;
    }

    public Core.Graphics.Fonts.Font ToGraphicsFont() =>
        new Core.Graphics.Fonts.Font(Data, $"{Name}-{Type}", Size);
}
