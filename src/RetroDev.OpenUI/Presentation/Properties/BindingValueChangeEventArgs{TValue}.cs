namespace RetroDev.OpenUI.Presentation.Properties;

/// <summary>
/// Argument for <see cref="IBinder{TValue}"/> value change event.
/// </summary>
/// <typeparam name="TValue">The change value type.</typeparam>
public class BinderValueChangeEventArgs<TValue> : EventArgs
{
    /// <summary>
    /// The updated value.
    /// </summary>
    public TValue Value { get; }

    /// <summary>
    /// Creates a new event arguent.
    /// </summary>
    /// <param name="value">The updated value.</param>
    public BinderValueChangeEventArgs(TValue value)
    {
        Value = value;
    }
}
