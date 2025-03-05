namespace RetroDev.UISharp.Core.Windowing;

/// <summary>
/// The mouse cursor shape to display.
/// </summary>
public enum MouseCursor
{
    /// <summary>
    /// The cursor shape is not a known shape in this enum.
    /// </summary>
    Unknown,

    /// <summary>
    /// The default cursor used in the current OS, typitcally an arrow.
    /// </summary>
    Default,

    /// <summary>
    /// The cursor used in editable text.
    /// </summary>
    Edit,

    /// <summary>
    /// The cursor indicating that the system is waiting.
    /// </summary>
    Waiting,

    /// <summary>
    /// Cross curor used for drag operations.
    /// </summary>
    Cross,

    /// <summary>
    /// Resize cursor.
    /// </summary>
    SizeNorthWestSouthEast,

    /// <summary>
    /// Resize cursor.
    /// </summary>
    SizeNorthEastSouthWest,

    /// <summary>
    /// Resize cursor.
    /// </summary>
    SizeWestEast,

    /// <summary>
    /// Resize cursor.
    /// </summary>
    SizeNorthSouth,

    /// <summary>
    /// Resize cursor.
    /// </summary>
    SizeAll,

    /// <summary>
    /// Hand cursor.
    /// </summary>
    Hand,

    /// <summary>
    /// No cursor.
    /// </summary>
    None
}
