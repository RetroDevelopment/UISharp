using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Logging;

namespace RetroDev.OpenUI.Core.Graphics.OpenGL;

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
