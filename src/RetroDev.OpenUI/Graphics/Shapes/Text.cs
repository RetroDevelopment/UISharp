namespace RetroDev.OpenUI.Graphics.Shapes;

public record Text(Color BackgroundColor, Color ForegroundColor, string Value)
{
    public int? TextureID { get; internal set; } = null;
}
