namespace RetroDev.UISharp.Core.Windowing.Events;

/// <summary>
/// Represents a keyboard key.
/// </summary>
public enum KeyButton
{
    /// <summary>
    /// Unknown button.
    /// </summary>
    Unknown,

    /// <summary>
    /// Represents the Return (Enter) key.
    /// </summary>
    Return,

    /// <summary>
    /// Represents the Escape (ESC) key.
    /// </summary>
    Escape,

    /// <summary>
    /// Represents the Backspace (delete) key.
    /// </summary>
    Backspace,

    /// <summary>
    /// Represents the Tab key.
    /// </summary>
    Tab,

    /// <summary>
    /// Represents the Space bar key.
    /// </summary>
    Space,

    /// <summary>
    /// Represents the ! (exclamation mark) key.
    /// </summary>
    Exclaim,

    /// <summary>
    /// Represents the " (double quote) key.
    /// </summary>
    DoubleQuote,


    /// <summary>
    /// Represents the # (hash) button.
    /// </summary>
    Hash,

    /// <summary>
    /// Represents the % (percent) key.
    /// </summary>
    Percent,

    /// <summary>
    /// Represents the $ (dollar) key.
    /// </summary>
    Dollar,

    /// <summary>
    /// Represents the & (ampersand) key.
    /// </summary>
    Ampersand,

    /// <summary>
    /// Represents the ' (apostrophe) key.
    /// </summary>
    Quote,

    /// <summary>
    /// Represents the ( (left parenthesis) key.
    /// </summary>
    LeftParenthesis,

    /// <summary>
    /// Represents the ) (right parenthesis) key.
    /// </summary>
    RightParenthesis,

    /// <summary>
    /// Represents the * (asterisk) key.
    /// </summary>
    Asterisk,

    /// <summary>
    /// Represents the + (plus) key.
    /// </summary>
    Plus,

    /// <summary>
    /// Represents the , (comma) key.
    /// </summary>
    Comma,

    /// <summary>
    /// Represents the - (minus) key.
    /// </summary>
    Minus,

    /// <summary>
    /// Represents the . (period) key.
    /// </summary>
    Period,

    /// <summary>
    /// Represents the / (slash) key.
    /// </summary>
    Slash,

    /// <summary>
    /// Represents the 0 (zero) key.
    /// </summary>
    Number0,

    /// <summary>
    /// Represents the 1 (one) key.
    /// </summary>
    Number1,

    /// <summary>
    /// Represents the 2 (two) key.
    /// </summary>
    Number2,

    /// <summary>
    /// Represents the 3 (three) key.
    /// </summary>
    Number3,

    /// <summary>
    /// Represents the 4 (four) key.
    /// </summary>
    Number4,

    /// <summary>
    /// Represents the 5 (five) key.
    /// </summary>
    Number5,

    /// <summary>
    /// Represents the 6 (six) key.
    /// </summary>
    Number6,

    /// <summary>
    /// Represents the 7 (seven) key.
    /// </summary>
    Number7,

    /// <summary>
    /// Represents the 8 (eight) key.
    /// </summary>
    Number8,

    /// <summary>
    /// Represents the 9 (nine) key.
    /// </summary>
    Number9,

    /// <summary>
    /// Represents the : (colon) key.
    /// </summary>
    Colon,

    /// <summary>
    /// Represents the ; (semicolon) key.
    /// </summary>
    Semicolon,

    /// <summary>
    /// Represents the &lt; (less than) key.
    /// </summary>
    LessThan,

    /// <summary>
    /// Represents the = (equals) key.
    /// </summary>
    Equals,

    /// <summary>
    /// Represents the &gt; (greater than) key.
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Represents the ? (question mark) key.
    /// </summary>
    QuestionMark,

    /// <summary>
    /// Represents the @ (at) key.
    /// </summary>
    At,

    /// <summary>
    /// Represents the [ (left square bracket) key.
    /// </summary>
    LeftSquareBracket,

    /// <summary>
    /// Represents the \ (backslash) key.
    /// </summary>
    Backslash,

    /// <summary>
    /// Represents the ] (right square bracket) key.
    /// </summary>
    RightSquareBracket,

    /// <summary>
    /// Represents the ^ (caret) key.
    /// </summary>
    Caret,

    /// <summary>
    /// Represents the _ (underscore) key.
    /// </summary>
    Underscore,

    /// <summary>
    /// Represents the ` (backtick) key.
    /// </summary>
    Backtick,

    /// <summary>
    /// Represents the a key.
    /// </summary>
    A,

    /// <summary>
    /// Represents the b key.
    /// </summary>
    B,

    /// <summary>
    /// Represents the c key.
    /// </summary>
    C,

    /// <summary>
    /// Represents the d key.
    /// </summary>
    D,

    /// <summary>
    /// Represents the e key.
    /// </summary>
    E,

    /// <summary>
    /// Represents the f key.
    /// </summary>
    F,

    /// <summary>
    /// Represents the g key.
    /// </summary>
    G,

    /// <summary>
    /// Represents the h key.
    /// </summary>
    H,

    /// <summary>
    /// Represents the i key.
    /// </summary>
    I,

    /// <summary>
    /// Represents the j key.
    /// </summary>
    J,

    /// <summary>
    /// Represents the k key.
    /// </summary>
    K,

    /// <summary>
    /// Represents the l key.
    /// </summary>
    L,

    /// <summary>
    /// Represents the m key.
    /// </summary>
    M,

    /// <summary>
    /// Represents the n key.
    /// </summary>
    N,

    /// <summary>
    /// Represents the o key.
    /// </summary>
    O,

    /// <summary>
    /// Represents the p key.
    /// </summary>
    P,

    /// <summary>
    /// Represents the q key.
    /// </summary>
    Q,

    /// <summary>
    /// Represents the r key.
    /// </summary>
    R,

    /// <summary>
    /// Represents the s key.
    /// </summary>
    S,

    /// <summary>
    /// Represents the t key.
    /// </summary>
    T,

    /// <summary>
    /// Represents the u key.
    /// </summary>
    U,

    /// <summary>
    /// Represents the v key.
    /// </summary>
    V,

    /// <summary>
    /// Represents the w key.
    /// </summary>
    W,

    /// <summary>
    /// Represents the x key.
    /// </summary>
    X,

    /// <summary>
    /// Represents the y key.
    /// </summary>
    Y,

    /// <summary>
    /// Represents the z key.
    /// </summary>
    Z,

    /// <summary>
    /// Represents the Caps Lock key.
    /// </summary>
    CapsLock,

    /// <summary>
    /// Represents the F1 key.
    /// </summary>
    F1,

    /// <summary>
    /// Represents the F2 key.
    /// </summary>
    F2,

    /// <summary>
    /// Represents the F3 key.
    /// </summary>
    F3,

    /// <summary>
    /// Represents the F4 key.
    /// </summary>
    F4,

    /// <summary>
    /// Represents the F5 key.
    /// </summary>
    F5,

    /// <summary>
    /// Represents the F6 key.
    /// </summary>
    F6,

    /// <summary>
    /// Represents the F7 key.
    /// </summary>
    F7,

    /// <summary>
    /// Represents the F8 key.
    /// </summary>
    F8,

    /// <summary>
    /// Represents the F9 key.
    /// </summary>
    F9,

    /// <summary>
    /// Represents the F10 key.
    /// </summary>
    F10,

    /// <summary>
    /// Represents the F11 key.
    /// </summary>
    F11,

    /// <summary>
    /// Represents the F12 key.
    /// </summary>
    F12,

    /// <summary>
    /// Represents the Print Screen key.
    /// </summary>
    PrintScreen,

    /// <summary>
    /// Represents the Scroll Lock key.
    /// </summary>
    ScrollLock,

    /// <summary>
    /// Represents the Pause key.
    /// </summary>
    Pause,

    /// <summary>
    /// Represents the Insert key.
    /// </summary>
    Insert,

    /// <summary>
    /// Represents the Home key.
    /// </summary>
    Home,

    /// <summary>
    /// Represents the Page Up key.
    /// </summary>
    PageUp,

    /// <summary>
    /// Represents the Delete key.
    /// </summary>
    Delete,

    /// <summary>
    /// Represents the End key.
    /// </summary>
    End,

    /// <summary>
    /// Represents the Page Down key.
    /// </summary>
    PageDown,

    /// <summary>
    /// Represents the Right Arrow key.
    /// </summary>
    Right,

    /// <summary>
    /// Represents the Left Arrow key.
    /// </summary>
    Left,

    /// <summary>
    /// Represents the Down Arrow key.
    /// </summary>
    Down,

    /// <summary>
    /// Represents the Up Arrow key.
    /// </summary>
    Up,
    /// <summary>
    /// Represents the Num Lock Clear key.
    /// </summary>
    NumLockClear,

    /// <summary>
    /// Represents the Keypad Divide key.
    /// </summary>
    KeyPadDivide,

    /// <summary>
    /// Represents the Keypad Multiply key.
    /// </summary>
    KeyPadMultiply,

    /// <summary>
    /// Represents the Keypad Minus key.
    /// </summary>
    KeyPadMinus,

    /// <summary>
    /// Represents the Keypad Plus key.
    /// </summary>
    KeyPadPlus,

    /// <summary>
    /// Represents the Keypad Enter key.
    /// </summary>
    KeyPadEnter,

    /// <summary>
    /// Represents the Keypad 1 key.
    /// </summary>
    KeyPad1,

    /// <summary>
    /// Represents the Keypad 2 key.
    /// </summary>
    KeyPad2,

    /// <summary>
    /// Represents the Keypad 3 key.
    /// </summary>
    KeyPad3,

    /// <summary>
    /// Represents the Keypad 4 key.
    /// </summary>
    KeyPad4,

    /// <summary>
    /// Represents the Keypad 5 key.
    /// </summary>
    KeyPad5,

    /// <summary>
    /// Represents the Keypad 6 key.
    /// </summary>
    KeyPad6,

    /// <summary>
    /// Represents the Keypad 7 key.
    /// </summary>
    KeyPad7,

    /// <summary>
    /// Represents the Keypad 8 key.
    /// </summary>
    KeyPad8,

    /// <summary>
    /// Represents the Keypad 9 key.
    /// </summary>
    KeyPad9,

    /// <summary>
    /// Represents the Keypad 0 key.
    /// </summary>
    KeyPad0,

    /// <summary>
    /// Represents the Keypad Period key.
    /// </summary>
    KeyPadPeriod,

    /// <summary>
    /// Represents the Application key.
    /// </summary>
    Application,

    /// <summary>
    /// Represents the Power key.
    /// </summary>
    Power,

    /// <summary>
    /// Represents the Keypad Equals key.
    /// </summary>
    KeyPadEquals,

    /// <summary>
    /// Represents the F13 key.
    /// </summary>
    F13,

    /// <summary>
    /// Represents the F14 key.
    /// </summary>
    F14,

    /// <summary>
    /// Represents the F15 key.
    /// </summary>
    F15,

    /// <summary>
    /// Represents the F16 key.
    /// </summary>
    F16,

    /// <summary>
    /// Represents the F17 key.
    /// </summary>
    F17,

    /// <summary>
    /// Represents the F18 key.
    /// </summary>
    F18,

    /// <summary>
    /// Represents the F19 key.
    /// </summary>
    F19,

    /// <summary>
    /// Represents the F20 key.
    /// </summary>
    F20,

    /// <summary>
    /// Represents the F21 key.
    /// </summary>
    F21,

    /// <summary>
    /// Represents the F22 key.
    /// </summary>
    F22,

    /// <summary>
    /// Represents the F23 key.
    /// </summary>
    F23,

    /// <summary>
    /// Represents the F24 key.
    /// </summary>
    F24,
    /// <summary>
    /// Represents the Execute key.
    /// </summary>
    Execute,

    /// <summary>
    /// Represents the Help key.
    /// </summary>
    Help,

    /// <summary>
    /// Represents the Menu key.
    /// </summary>
    Menu,

    /// <summary>
    /// Represents the Select key.
    /// </summary>
    Select,

    /// <summary>
    /// Represents the Stop key.
    /// </summary>
    Stop,

    /// <summary>
    /// Represents the Again key.
    /// </summary>
    Again,

    /// <summary>
    /// Represents the Undo key.
    /// </summary>
    Undo,

    /// <summary>
    /// Represents the Cut key.
    /// </summary>
    Cut,

    /// <summary>
    /// Represents the Copy key.
    /// </summary>
    Copy,

    /// <summary>
    /// Represents the Paste key.
    /// </summary>
    Paste,

    /// <summary>
    /// Represents the Find key.
    /// </summary>
    Find,

    /// <summary>
    /// Represents the Mute key.
    /// </summary>
    Mute,

    /// <summary>
    /// Represents the Volume Up key.
    /// </summary>
    VolumeUp,

    /// <summary>
    /// Represents the Volume Down key.
    /// </summary>
    VolumeDown,

    /// <summary>
    /// Represents the Keypad Comma key.
    /// </summary>
    KeyPadComma,

    /// <summary>
    /// Represents the Keypad Equals AS400 key.
    /// </summary>
    KeyPadEqualsAS400,

    /// <summary>
    /// Represents the Alternate Erase key.
    /// </summary>
    AltErase,

    /// <summary>
    /// Represents the SysReq key.
    /// </summary>
    SysReq,

    /// <summary>
    /// Represents the Cancel key.
    /// </summary>
    Cancel,

    /// <summary>
    /// Represents the Clear key.
    /// </summary>
    Clear,

    /// <summary>
    /// Represents the Prior key.
    /// </summary>
    Prior,

    /// <summary>
    /// Represents the Return 2 key.
    /// </summary>
    Return2,

    /// <summary>
    /// Represents the Separator key.
    /// </summary>
    Separator,

    /// <summary>
    /// Represents the Out key.
    /// </summary>
    Out,

    /// <summary>
    /// Represents the Oper key.
    /// </summary>
    Oper,

    /// <summary>
    /// Represents the Clear Again key.
    /// </summary>
    ClearAgain,

    /// <summary>
    /// Represents the CR Sel key.
    /// </summary>
    CRSel,

    /// <summary>
    /// Represents the Ex Sel key.
    /// </summary>
    ExSel,

    /// <summary>
    /// Represents the Keypad 00 key.
    /// </summary>
    KeyPad00,

    /// <summary>
    /// Represents the Keypad 000 key.
    /// </summary>
    KeyPad000,

    /// <summary>
    /// Represents the Thousands Separator key.
    /// </summary>
    ThousandsSeparator,

    /// <summary>
    /// Represents the Decimal Separator key.
    /// </summary>
    DecimalSeparator,

    /// <summary>
    /// Represents the Currency Unit key.
    /// </summary>
    CurrencyUnit,

    /// <summary>
    /// Represents the Currency Subunit key.
    /// </summary>
    CurrencySubunit,

    /// <summary>
    /// Represents the Keypad Left Parenthesis key.
    /// </summary>
    KeyPadLeftParen,

    /// <summary>
    /// Represents the Keypad Right Parenthesis key.
    /// </summary>
    KeyPadRightParen,

    /// <summary>
    /// Represents the Keypad Left Brace key.
    /// </summary>
    KeyPadLeftBrace,

    /// <summary>
    /// Represents the Keypad Right Brace key.
    /// </summary>
    KeyPadRightBrace,

    /// <summary>
    /// Represents the Keypad Tab key.
    /// </summary>
    KeyPadTab,

    /// <summary>
    /// Represents the Keypad Backspace key.
    /// </summary>
    KeyPadBackspace,

    /// <summary>
    /// Represents the Keypad A key.
    /// </summary>
    KeyPadA,

    /// <summary>
    /// Represents the Keypad B key.
    /// </summary>
    KeyPadB,

    /// <summary>
    /// Represents the Keypad C key.
    /// </summary>
    KeyPadC,

    /// <summary>
    /// Represents the Keypad D key.
    /// </summary>
    KeyPadD,

    /// <summary>
    /// Represents the Keypad E key.
    /// </summary>
    KeyPadE,

    /// <summary>
    /// Represents the Keypad F key.
    /// </summary>
    KeyPadF,

    /// <summary>
    /// Represents the Keypad XOR key.
    /// </summary>
    KeyPadXOR,

    /// <summary>
    /// Represents the Keypad Power key.
    /// </summary>
    KeyPadPower,

    /// <summary>
    /// Represents the Keypad Percent key.
    /// </summary>
    KeyPadPercent,

    /// <summary>
    /// Represents the Keypad Less key.
    /// </summary>
    KeyPadLess,

    /// <summary>
    /// Represents the Keypad Greater key.
    /// </summary>
    KeyPadGreater,

    /// <summary>
    /// Represents the Keypad Ampersand key.
    /// </summary>
    KeyPadAmpersand,

    /// <summary>
    /// Represents the Keypad Double Ampersand key.
    /// </summary>
    KeyPadDoubleAmpersand,

    /// <summary>
    /// Represents the Keypad Vertical Bar key.
    /// </summary>
    KeyPadVerticalBar,

    /// <summary>
    /// Represents the Keypad Double Vertical Bar key.
    /// </summary>
    KeyPadDoubleVerticalBar,

    /// <summary>
    /// Represents the Keypad Colon key.
    /// </summary>
    KeyPadColon,

    /// <summary>
    /// Represents the Keypad Hash key.
    /// </summary>
    KeyPadHash,

    /// <summary>
    /// Represents the Keypad Space key.
    /// </summary>
    KeyPadSpace,

    /// <summary>
    /// Represents the Keypad At key.
    /// </summary>
    KeyPadAt,

    /// <summary>
    /// Represents the Keypad Exclamation key.
    /// </summary>
    KeyPadExclam,
    /// <summary>
    /// Represents the Keypad Mem Store key.
    /// </summary>
    KeyPadMemStore,

    /// <summary>
    /// Represents the Keypad Mem Recall key.
    /// </summary>
    KeyPadMemRecall,

    /// <summary>
    /// Represents the Keypad Mem Clear key.
    /// </summary>
    KeyPadMemClear,

    /// <summary>
    /// Represents the Keypad Mem Add key.
    /// </summary>
    KeyPadMemAdd,

    /// <summary>
    /// Represents the Keypad Mem Subtract key.
    /// </summary>
    KeyPadMemSubtract,

    /// <summary>
    /// Represents the Keypad Mem Multiply key.
    /// </summary>
    KeyPadMemMultiply,

    /// <summary>
    /// Represents the Keypad Mem Divide key.
    /// </summary>
    KeyPadMemDivide,

    /// <summary>
    /// Represents the Keypad Plus-Minus key.
    /// </summary>
    KeyPadPlusMinus,

    /// <summary>
    /// Represents the Keypad Clear key.
    /// </summary>
    KeyPadClear,

    /// <summary>
    /// Represents the Keypad Clear Entry key.
    /// </summary>
    KeyPadClearEntry,

    /// <summary>
    /// Represents the Keypad Binary key.
    /// </summary>
    KeyPadBinary,

    /// <summary>
    /// Represents the Keypad Octal key.
    /// </summary>
    KeyPadOctal,

    /// <summary>
    /// Represents the Keypad Decimal key.
    /// </summary>
    KeyPadDecimal,

    /// <summary>
    /// Represents the Keypad Hexadecimal key.
    /// </summary>
    KeyPadHexadecimal,

    /// <summary>
    /// Represents the Left Control key.
    /// </summary>
    LeftControl,

    /// <summary>
    /// Represents the Left Shift key.
    /// </summary>
    LeftShift,

    /// <summary>
    /// Represents the Left Alt key.
    /// </summary>
    LeftAlt,

    /// <summary>
    /// Represents the Left GUI key.
    /// </summary>
    LeftGUI,

    /// <summary>
    /// Represents the Right Control key.
    /// </summary>
    RightControl,

    /// <summary>
    /// Represents the Right Shift key.
    /// </summary>
    RightShift,

    /// <summary>
    /// Represents the Right Alt key.
    /// </summary>
    RightAlt,

    /// <summary>
    /// Represents the Right GUI key.
    /// </summary>
    RightGUI,

    /// <summary>
    /// Represents the Mode key.
    /// </summary>
    Mode,

    /// <summary>
    /// Represents the Audio Next key.
    /// </summary>
    AudioNext,

    /// <summary>
    /// Represents the Audio Previous key.
    /// </summary>
    AudioPrev,

    /// <summary>
    /// Represents the Audio Stop key.
    /// </summary>
    AudioStop,

    /// <summary>
    /// Represents the Audio Play key.
    /// </summary>
    AudioPlay,

    /// <summary>
    /// Represents the Audio Mute key.
    /// </summary>
    AudioMute,

    /// <summary>
    /// Represents the Media Select key.
    /// </summary>
    MediaSelect,

    /// <summary>
    /// Represents the WWW key.
    /// </summary>
    WWW,

    /// <summary>
    /// Represents the Mail key.
    /// </summary>
    Mail,

    /// <summary>
    /// Represents the Calculator key.
    /// </summary>
    Calculator,

    /// <summary>
    /// Represents the Computer key.
    /// </summary>
    Computer,

    /// <summary>
    /// Represents the AC Search key.
    /// </summary>
    ACSearch,

    /// <summary>
    /// Represents the AC Home key.
    /// </summary>
    ACHome,

    /// <summary>
    /// Represents the AC Back key.
    /// </summary>
    ACBack,

    /// <summary>
    /// Represents the AC Forward key.
    /// </summary>
    ACForward,

    /// <summary>
    /// Represents the AC Stop key.
    /// </summary>
    ACStop,

    /// <summary>
    /// Represents the AC Refresh key.
    /// </summary>
    ACRefresh,

    /// <summary>
    /// Represents the AC Bookmarks key.
    /// </summary>
    ACBookmarks,

    /// <summary>
    /// Represents the Brightness Down key.
    /// </summary>
    BrightnessDown,

    /// <summary>
    /// Represents the Brightness Up key.
    /// </summary>
    BrightnessUp,

    /// <summary>
    /// Represents the Display Switch key.
    /// </summary>
    DisplaySwitch,

    /// <summary>
    /// Represents the Keypad Illumination Toggle key.
    /// </summary>
    KeyPadIllumToggle,

    /// <summary>
    /// Represents the Keypad Illumination Down key.
    /// </summary>
    KeyPadIllumDown,

    /// <summary>
    /// Represents the Keypad Illumination Up key.
    /// </summary>
    KeyPadIllumUp,

    /// <summary>
    /// Represents the Eject key.
    /// </summary>
    Eject,

    /// <summary>
    /// Represents the Sleep key.
    /// </summary>
    Sleep,

    /// <summary>
    /// Represents the App 1 key.
    /// </summary>
    App1,

    /// <summary>
    /// Represents the App 2 key.
    /// </summary>
    App2,

    /// <summary>
    /// Represents the Audio Rewind key.
    /// </summary>
    AudioRewind,

    /// <summary>
    /// Represents the Audio Fast Forward key.
    /// </summary>
    AudioFastForward,
}
