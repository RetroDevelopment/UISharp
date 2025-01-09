using System.Linq.Expressions;
using System.Security.Cryptography;
using OpenTK.Graphics.OpenGL;
using RetroDev.OpenUI.Events;
using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Logging;
using SDL2;

namespace RetroDev.OpenUI.Utils;

internal static class LoggingUtils
{
    public static void SDLCheck(Expression<Func<int>> action, ILogger logger, bool warning = false)
    {
        var returnValue = InvokeAndLog(action, logger);
        if (returnValue != 0 && !warning) throw new RenderingException($"Error occurred when rendering: {SDL.SDL_GetError()}");
        if (returnValue != 0 && warning) logger.LogWarning($"Error occurred when rendering: {SDL.SDL_GetError()}. Operation was {action.Body}");
    }

    /// <summary>
    /// Logs the exact representation of a lambda expression and its result.
    /// </summary>
    /// <param name="action">A lambda expression representing the method to be invoked.</param>
    /// <returns>The result of the invoked method.</returns>
    public static int InvokeAndLog(Expression<Func<int>> action, ILogger logger)
    {
        logger.LogVerbose($"Invoking {action.Body}");
        var compiled = action.Compile();
        var result = compiled();
        logger.LogVerbose($"Result = {result}");
        return result;
    }

    /// <summary>
    /// Logs the status of shader operations.
    /// </summary>
    /// <param name="shaderOperation">The shader operation.</param>
    /// <param name="shaderId">The id of the opengl shader.</param>
    /// <param name="logger">The logger.</param>
    public static void LogShaderStatus(string shaderOperation, int shaderId, ILogger logger)
    {
        var shaderLogOutput = GL.GetProgramInfoLog(shaderId);
        if (string.IsNullOrEmpty(shaderLogOutput))
        {
            logger.LogDebug($"Operation {shaderOperation} succeeded!");
        }
        else
        {
            logger.LogError($"Error {shaderOperation}: {shaderLogOutput}");
        }
    }

    /// <summary>
    /// Logs this event.
    /// </summary>
    /// <param name="this">The event arguments to log.</param>
    /// <param name="eventName">The name of this event.</param>
    public static void Log(this EventArgs @this, string eventName, ILogger logger)
    {
        logger.LogVerbose($"Raised event {eventName}({@this})");
    }

    /// <summary>
    /// Logs this event.
    /// </summary>
    /// <param name="this">The event arguments to log.</param>
    /// <param name="eventName">The name of this event.</param>
    public static void Log<TArgs>(this WindowEventArgs<TArgs> @this, string eventName, ILogger logger) where TArgs : EventArgs
    {
        logger.LogVerbose($"Raised event {eventName}(window={@this.WindowId}, {@this.Args})");
    }
}
