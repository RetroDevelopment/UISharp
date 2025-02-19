using RetroDev.OpenUI.Core.Graphics.Fonts;

namespace RetroDev.OpenUI.Core.Graphics.Shapes;

public record Text(Color BackgroundColor, Color ForegroundColor, string Value, Font Font)
{
    public int? TextureID { get; internal set; } = null;
}
