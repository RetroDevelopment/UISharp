using OpenTK.Graphics.OpenGL;

namespace RetroDev.OpenUI.Graphics.Internal.OpenGL;

internal class Shader
{
    public int ID { get; }

    public Shader(ShaderType type, string code)
    {
        ID = GL.CreateShader(type);
        GL.ShaderSource(ID, code);
        GL.CompileShader(ID);

        Console.WriteLine($"{type} logs: " + GL.GetShaderInfoLog(ID));
    }

    public void Close()
    {
        GL.DeleteShader(ID);
    }
}
