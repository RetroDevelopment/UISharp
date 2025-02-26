namespace RetroDev.OpenUI.Core.Exceptions;

/// <summary>
/// An exception that occurs when performing an operation in an invalid lifecycle state.
/// </summary>
internal class LifeCycleException(string message) : UIException(message)
{
}
