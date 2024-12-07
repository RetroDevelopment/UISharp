using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

/// <summary>
/// Represents a 2D area size.
/// </summary>
/// <param name="Width">The width in pixels.</param>
/// <param name="Height">The height in pixels.</param>
[DebuggerDisplay("{Width} x {Height}")]
public record Size(PixelUnit Width, PixelUnit Height)
{
    public static readonly Size Zero = new(0, 0);
}
