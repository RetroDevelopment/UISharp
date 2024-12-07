using RetroDev.OpenUI.Exceptions;
using SDL2;

namespace RetroDev.OpenUI.Core.Internal;

/// <summary>
/// Contains UI environment information and performs generic UI environment operations such as initializing the UI.
/// </summary>
internal class SDLUIEnvironment : IUIEnvironment
{
    private static bool s_isInitialized = false;
    private static object s_lock = new();

    /// <summary>
    /// Initializes the UI environment.
    /// </summary>
    /// <exception cref="UIInitializationException">If the initialization fails or the environment has already been initialized.</exception>
    public void Initialize()
    {
        lock (s_lock)
        {
            if (s_isInitialized)
            {
                throw new UIInitializationException("SDL environment can only be initialized once and in one thread.");
            }

            var initResult = SDL.SDL_InitSubSystem(SDL.SDL_INIT_VIDEO);
            if (initResult < 0)
            {
                throw new UIInitializationException($"Error initializing SDL: {SDL.SDL_GetError()}");
            }

            s_isInitialized = true;
        }
    }

    /// <summary>
    /// Closes the SDL environment. Make sure that no other SDL operation is performed after calling this method!
    /// </summary>
    public void Shutdown()
    {
        SDL.SDL_Quit();
    }
}
