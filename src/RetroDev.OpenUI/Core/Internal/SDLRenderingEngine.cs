using System.Runtime.InteropServices;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Exceptions;
using SDL2;

namespace RetroDev.OpenUI.Core.Internal;

/// <summary>
/// The SDL rendering engine used to render on a given window.
/// </summary>
internal class SDLRenderingEngine : IRenderingEngine
{
    private record Texture(IntPtr SDLTexture, int SizeBytes, int Width, int Height);

    LifeCycle _lifeCycle;
    private readonly IntPtr _renderer;
    private readonly Dictionary<string, Texture> _textureCache = [];

    public SDLRenderingEngine(LifeCycle lifeCycle, IntPtr window)
    {
        _lifeCycle = lifeCycle;
        _renderer = SDL.SDL_CreateRenderer(window, -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
        if (_renderer == IntPtr.Zero) throw new UIInitializationException($"Error creating renderer: {SDL.SDL_GetError()}");
    }

    /// <summary>
    /// Creates and caches a texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="image">The rgba image.</param>
    /// <exception cref="ArgumentException">If a texture with the same <paramref name="textureIdentifier"/> exists or SDL fails to create the texture.</exception>
    /// <exception cref="RenderingException">If the texture cannot be created because of a system error.</exception>
    public void CreateTexture(string textureIdentifier, RgbaImage image)
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();

        if (_textureCache.ContainsKey(textureIdentifier)) throw new RenderingException($"Texture with id '{textureIdentifier}' has already been created");
        var expectedSize = image.Width * image.Height * 4;
        if (image.Data.Length != expectedSize) throw new ArgumentException($"Could not create SDL texture with id '{textureIdentifier}': expected an rgba array of size {expectedSize}, found an array of size {image.Data.Length}");

        var pixelFormat = BitConverter.IsLittleEndian ? SDL.SDL_PIXELFORMAT_ABGR8888 : SDL.SDL_PIXELFORMAT_RGBA8888;
        var texture = SDL.SDL_CreateTexture(_renderer,
                                            pixelFormat,
                                            (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING,
                                            image.Width,
                                            image.Height);

        if (texture == IntPtr.Zero) throw new RenderingException($"Could not create SDL texture {textureIdentifier}");

        SDLCheck(() => SDL.SDL_SetTextureBlendMode(texture, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND));

        var pixels = IntPtr.Zero;
        var pitch = 0;
        SDLCheck(() => SDL.SDL_LockTexture(texture, IntPtr.Zero, out pixels, out pitch));
        FlushTexture(image.Data, pixels, image.Width, image.Height, pitch);
        SDL.SDL_UnlockTexture(texture);

        _textureCache.Add(textureIdentifier, new(texture, image.Data.Length, image.Width, image.Height));
    }

    /// <summary>
    /// Updates an existing texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="image">The rgba image.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist or the rgba array doesn't have the same size as the originally created texture.</exception>.
    /// <exception cref="RenderingException">If the texture cannot be updated because of a system error.</exception>
    public void UpdateTexture(string textureIdentifier, RgbaImage image)
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();

        if (!_textureCache.TryGetValue(textureIdentifier, out var texture)) throw new RenderingException($"Cannot update texture with id '{textureIdentifier}', the texture doesn't exist");

        if (texture.SizeBytes != image.Data.Length) throw new ArgumentException($"Failed to update texture with id '{textureIdentifier}', the texture has size {texture.SizeBytes} bytes, but a rgba array with size {image.Data.Length} bytes was provided insted");
        var pixels = IntPtr.Zero;
        var pitch = 0;
        SDLCheck(() => SDL.SDL_LockTexture(texture.SDLTexture, IntPtr.Zero, out pixels, out pitch));
        FlushTexture(image.Data, pixels, texture.Width, texture.Height, pitch);
        SDL.SDL_UnlockTexture(texture.SDLTexture);
    }

    /// <summary>
    /// Deletes an existing texture.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    public void DeleteTexture(string textureIdentifier)
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();

        if (!_textureCache.TryGetValue(textureIdentifier, out var texture)) throw new ArgumentException($"Cannot delete texture with id '{textureIdentifier}', the texture doesn't exist");
        SDL.SDL_DestroyTexture(texture.SDLTexture);
    }

    /// <summary>
    /// Checks whether the given texture has been created with <see cref="CreateTexture(string, byte[], CoordinateType, CoordinateType)"/>.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <returns><see langword="true"/> if the texture exists, otherwise <see langword="false"/>.</returns>
    public bool TextureExists(string textureIdentifier)
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();

        return _textureCache.ContainsKey(textureIdentifier);
    }

    /// <summary>
    /// Renders a texture in the viewport.
    /// </summary>
    /// <param name="textureIdentifier">The unique texture identifier.</param>
    /// <param name="renderingArea">The area where to render the texture. If the area size is differnet from the texture size the image will be streteched.</param>
    /// <param name="clippingArea">The area where the texture can be rendered. All the pixels outside of that area will be clipped.</param>
    /// <exception cref="ArgumentException">If the texture with <paramref name="textureIdentifier"/> doesn't exist.</exception>
    public void RenderTexture(string textureIdentifier, Area renderingArea, Area clippingArea)
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();

        if (!_textureCache.TryGetValue(textureIdentifier, out var value)) throw new ArgumentException($"Cannot render texture with id '{textureIdentifier}', the texture doesn't exist");
        var texture = value.SDLTexture;
        var sdlRenderingArea = new SDL.SDL_Rect
        {
            x = renderingArea.TopLeft.X,
            y = renderingArea.TopLeft.Y,
            w = renderingArea.Size.Width,
            h = renderingArea.Size.Height
        };

        var sdlClippingArea = new SDL.SDL_Rect
        {
            x = clippingArea.TopLeft.X,
            y = clippingArea.TopLeft.Y,
            w = clippingArea.Size.Width,
            h = clippingArea.Size.Height
        };

        SDL.SDL_SetHint(SDL.SDL_HINT_RENDER_SCALE_QUALITY, "1"); // Texture linear filtering
        SDLCheck(() => SDL.SDL_RenderSetClipRect(_renderer, ref sdlClippingArea));
        SDLCheck(() => SDL.SDL_RenderCopy(_renderer, texture, IntPtr.Zero, ref sdlRenderingArea));
        SDLCheck(() => SDL.SDL_RenderSetClipRect(_renderer, IntPtr.Zero));
    }

    /// <summary>
    /// This method is invoked when starting the rendering of a frame.
    /// </summary>
    public void InitializeFrame()
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();

        // black color
        SDLCheck(() => SDL.SDL_SetRenderDrawColor(_renderer, 0, 0, 0, 255));
        SDLCheck(() => SDL.SDL_RenderClear(_renderer));
    }

    /// <summary>
    /// This method is called when the frame rendering is complete.
    /// </summary>
    public void FinalizeFrame()
    {
        _lifeCycle.ThrowIfNotOnUIThread();
        _lifeCycle.ThrowIfNotOnRenderingPhase();
        SDL.SDL_RenderPresent(_renderer);
    }

    /// <summary>
    /// Dispose the rendering engine and deallocates all the graphical resources.
    /// </summary>
    public void Shutdown()
    {
        _lifeCycle.ThrowIfNotOnUIThread();

        foreach (var item in _textureCache)
        {
            if (item.Value.SDLTexture != IntPtr.Zero) DeleteTexture(item.Key);
        }

        if (_renderer != IntPtr.Zero) SDL.SDL_DestroyRenderer(_renderer);
    }

    private void FlushTexture(byte[] rgba, nint pixels, int width, int height, int pitch)
    {
        const int bytesPerPixel = 4; // Assuming 4 bytes per pixel (RGBA)

        if (pitch < width * bytesPerPixel) throw new RenderingException($"Texture with width {width} is expected to have a least {width * bytesPerPixel} bytes, but pitch is {pitch}");
        if (pitch == width * bytesPerPixel)
        {
            Marshal.Copy(rgba, 0, pixels, rgba.Length);
            return;
        }

        for (var y = 0; y < height; y++)
        {
            var sourceOffset = y * width * bytesPerPixel;
            var destPtr = pixels + y * pitch;
            Marshal.Copy(rgba, sourceOffset, destPtr, width * bytesPerPixel);
        }
    }

    private void SDLCheck(Func<int> action)
    {
        var returnValue = action();
        if (returnValue != 0) throw new RenderingException($"Error occurred when rendering: {SDL.SDL_GetError()}");
    }
}
