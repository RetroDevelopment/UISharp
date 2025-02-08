namespace RetroDev.OpenUI.UI.Properties;

/// <summary>
/// Argument that notifies when a property value changes.
/// </summary>
/// <typeparam name="TValue">The proeprty value type.</typeparam>
public class ValueChangeEventArgs<TValue> : EventArgs
{
    /// <summary>
    /// Creates a new value change event argument.
    /// </summary>
    /// <param name="previousValue">The value before the update.</param>
    /// <param name="currentValue">The value after the update (current value).</param>
    public ValueChangeEventArgs(TValue previousValue, TValue currentValue)
    {
        PreviousValue = previousValue;
        CurrentValue = currentValue;
    }

    /// <summary>
    /// The value before the update.
    /// </summary>
    public TValue PreviousValue { get; }

    /// <summary>
    /// The value after the update (current value).
    /// </summary>
    public TValue CurrentValue { get; }
}
