namespace RetroDev.OpenUI.Exceptions;

/// <summary>
/// Indicates that a UI operation has been performed on a thread that is not the UI thread.
/// </summary>
public class InvalidUIThreadException() : UIException("UI operation must be performed on the UI thread");
