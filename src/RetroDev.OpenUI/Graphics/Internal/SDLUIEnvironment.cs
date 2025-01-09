using RetroDev.OpenUI.Exceptions;
using RetroDev.OpenUI.Utils;
using SDL2;

namespace RetroDev.OpenUI.Core.Internal;

/// <summary>
/// Contains UI environment information and performs generic UI environment operations such as initializing the UI.
/// </summary>
/// <param name="application">The application using this environment.</param>
internal class SDLUIEnvironment(Application application) : IUIEnvironment
{
    private static bool s_isInitialized = false;
    private static object s_lock = new();

    private Application _application = application;

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

            LoggingUtils.SDLCheck(() => SDL.SDL_InitSubSystem(SDL.SDL_INIT_VIDEO), _application.Logger);
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
