using System.ComponentModel;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.UIDefinition.Ast;
using static SDL2.SDL;

namespace RetroDev.UISharp.Core.Windowing.SDL;

/// <summary>
/// Maps the SDL system cursor defined in <see cref="SDL_SystemCursor"/> to <see cref="MouseCursor"/>.
/// </summary>
public static class SDLMouseCursorMapping
{
    private static readonly Dictionary<MouseCursor, SDL_SystemCursor> s_cursorMapping = new()
    {
        { MouseCursor.Default, SDL_SystemCursor.SDL_SYSTEM_CURSOR_ARROW },
        { MouseCursor.Edit, SDL_SystemCursor.SDL_SYSTEM_CURSOR_IBEAM },
        { MouseCursor.Waiting, SDL_SystemCursor.SDL_SYSTEM_CURSOR_WAIT },
        { MouseCursor.Cross, SDL_SystemCursor.SDL_SYSTEM_CURSOR_CROSSHAIR },
        { MouseCursor.SizeNorthWestSouthEast, SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENWSE},
        { MouseCursor.SizeNorthEastSouthWest, SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENESW },
        { MouseCursor.SizeWestEast, SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEWE },
        { MouseCursor.SizeNorthSouth, SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZENS },
        {MouseCursor.SizeAll, SDL_SystemCursor.SDL_SYSTEM_CURSOR_SIZEALL },
        {MouseCursor.Hand, SDL_SystemCursor.SDL_SYSTEM_CURSOR_HAND },
        {MouseCursor.None, SDL_SystemCursor.SDL_SYSTEM_CURSOR_NO }
    };

    /// <summary>
    /// Returns the <see cref="SDL_SystemCursor"/> equivalent to the given <paramref name="cursor"/>.
    /// </summary>
    /// <param name="cursor">The system mouse cursor to convert.</param>
    /// <returns>The equivalent <see cref="SDL_SystemCursor"/>.</returns>
    /// <remarks>
    /// Having a SDL enum and a framework equivlent enum is necessary since all SDL specific code is
    /// provided by interface implementation and SDL can be replaced by other technologies.
    /// </remarks>
    public static SDL_SystemCursor ToKeyButton(MouseCursor cursor)
    {
        return s_cursorMapping.TryGetValue(cursor, out var sdlCursor) ?
            sdlCursor :
            throw new InvalidEnumArgumentException($"Failed to convert {cursor} into an SDL enum");
    }
}
