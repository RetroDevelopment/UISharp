using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Logging;
using RetroDev.OpenUI.Utils;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal class Shader
{
    public int ID { get; }

    public Shader(ShaderType type, string code, ILogger logger)
    {
        ID = GL.CreateShader(type);
        GL.ShaderSource(ID, code);
        GL.CompileShader(ID);
        LoggingUtils.LogShaderStatus($"loading {type}", ID, logger);
    }

    public void Close()
    {
        GL.DeleteShader(ID);
    }
}
