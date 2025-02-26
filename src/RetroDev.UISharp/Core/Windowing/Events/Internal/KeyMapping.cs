using RetroDev.UISharp.Core.Windowing.Events;
using static SDL2.SDL;

namespace RetroDev.UISharp.Core.Windowing.Events.Internal;

internal static class KeyMapping
{
    private static readonly Dictionary<SDL_Keycode, KeyButton> sdlToKeyButtonMap = new Dictionary<SDL_Keycode, KeyButton>
    {
        { SDL_Keycode.SDLK_UNKNOWN, KeyButton.Unknown },
        { SDL_Keycode.SDLK_RETURN, KeyButton.Return },
        { SDL_Keycode.SDLK_ESCAPE, KeyButton.Escape },
        { SDL_Keycode.SDLK_BACKSPACE, KeyButton.Backspace },
        { SDL_Keycode.SDLK_TAB, KeyButton.Tab },
        { SDL_Keycode.SDLK_SPACE, KeyButton.Space },
        { SDL_Keycode.SDLK_EXCLAIM, KeyButton.Exclaim },
        { SDL_Keycode.SDLK_QUOTEDBL, KeyButton.Quotedbl },
        { SDL_Keycode.SDLK_HASH, KeyButton.Hash },
        { SDL_Keycode.SDLK_DOLLAR, KeyButton.Dollar },
        { SDL_Keycode.SDLK_AMPERSAND, KeyButton.Ampersand },
        { SDL_Keycode.SDLK_QUOTE, KeyButton.Quote },
        { SDL_Keycode.SDLK_LEFTPAREN, KeyButton.LeftParen },
        { SDL_Keycode.SDLK_RIGHTPAREN, KeyButton.RightParen },
        { SDL_Keycode.SDLK_ASTERISK, KeyButton.Asterisk },
        { SDL_Keycode.SDLK_PLUS, KeyButton.Plus },
        { SDL_Keycode.SDLK_COMMA, KeyButton.Comma },
        { SDL_Keycode.SDLK_MINUS, KeyButton.Minus },
        { SDL_Keycode.SDLK_PERIOD, KeyButton.Period },
        { SDL_Keycode.SDLK_SLASH, KeyButton.Slash },
        { SDL_Keycode.SDLK_0, KeyButton.Num0 },
        { SDL_Keycode.SDLK_1, KeyButton.Num1 },
        { SDL_Keycode.SDLK_2, KeyButton.Num2 },
        { SDL_Keycode.SDLK_3, KeyButton.Num3 },
        { SDL_Keycode.SDLK_4, KeyButton.Num4 },
        { SDL_Keycode.SDLK_5, KeyButton.Num5 },
        { SDL_Keycode.SDLK_6, KeyButton.Num6 },
        { SDL_Keycode.SDLK_7, KeyButton.Num7 },
        { SDL_Keycode.SDLK_8, KeyButton.Num8 },
        { SDL_Keycode.SDLK_9, KeyButton.Num9 },
        { SDL_Keycode.SDLK_a, KeyButton.A },
        { SDL_Keycode.SDLK_b, KeyButton.B },
        { SDL_Keycode.SDLK_c, KeyButton.C },
        { SDL_Keycode.SDLK_d, KeyButton.D },
        { SDL_Keycode.SDLK_e, KeyButton.E },
        { SDL_Keycode.SDLK_f, KeyButton.F },
        { SDL_Keycode.SDLK_g, KeyButton.G },
        { SDL_Keycode.SDLK_h, KeyButton.H },
        { SDL_Keycode.SDLK_i, KeyButton.I },
        { SDL_Keycode.SDLK_j, KeyButton.J },
        { SDL_Keycode.SDLK_k, KeyButton.K },
        { SDL_Keycode.SDLK_l, KeyButton.L },
        { SDL_Keycode.SDLK_m, KeyButton.M },
        { SDL_Keycode.SDLK_n, KeyButton.N },
        { SDL_Keycode.SDLK_o, KeyButton.O },
        { SDL_Keycode.SDLK_p, KeyButton.P },
        { SDL_Keycode.SDLK_q, KeyButton.Q },
        { SDL_Keycode.SDLK_r, KeyButton.R },
        { SDL_Keycode.SDLK_s, KeyButton.S },
        { SDL_Keycode.SDLK_t, KeyButton.T },
        { SDL_Keycode.SDLK_u, KeyButton.U },
        { SDL_Keycode.SDLK_v, KeyButton.V },
        { SDL_Keycode.SDLK_w, KeyButton.W },
        { SDL_Keycode.SDLK_x, KeyButton.X },
        { SDL_Keycode.SDLK_y, KeyButton.Y },
        { SDL_Keycode.SDLK_z, KeyButton.Z },
        { SDL_Keycode.SDLK_F1, KeyButton.F1 },
        { SDL_Keycode.SDLK_F2, KeyButton.F2 },
        { SDL_Keycode.SDLK_F3, KeyButton.F3 },
        { SDL_Keycode.SDLK_F4, KeyButton.F4 },
        { SDL_Keycode.SDLK_F5, KeyButton.F5 },
        { SDL_Keycode.SDLK_F6, KeyButton.F6 },
        { SDL_Keycode.SDLK_F7, KeyButton.F7 },
        { SDL_Keycode.SDLK_F8, KeyButton.F8 },
        { SDL_Keycode.SDLK_F9, KeyButton.F9 },
        { SDL_Keycode.SDLK_F10, KeyButton.F10 },
        { SDL_Keycode.SDLK_F11, KeyButton.F11 },
        { SDL_Keycode.SDLK_F12, KeyButton.F12 },
        { SDL_Keycode.SDLK_CAPSLOCK, KeyButton.CapsLock },
        { SDL_Keycode.SDLK_LSHIFT, KeyButton.LeftShift },
        { SDL_Keycode.SDLK_RSHIFT, KeyButton.RightShift },
        // Add other mappings as necessary
    };

    public static KeyButton ToKeyButton(SDL_Keycode sdlKeycode)
    {
        return sdlToKeyButtonMap.TryGetValue(sdlKeycode, out var keyButton) ? keyButton : KeyButton.Unknown;
    }
}
