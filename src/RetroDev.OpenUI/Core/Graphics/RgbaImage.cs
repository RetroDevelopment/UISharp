namespace RetroDev.OpenUI.Core.Graphics;

/// <summary>
/// An image in the RGBA format. Each 4 byte group represents a pixel red, green, blue, alpha values.
/// </summary>
/// <param name="Data">The image rgba data.</param>
/// <param name="Width">The image width in pixels.</param>
/// <param name="Height">The image height in pixels.</param>
public record RgbaImage(byte[] Data, int Width, int Height);
