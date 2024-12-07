namespace RetroDev.OpenUI.Properties;

public class ValueChangeEventArgs<TValue> : EventArgs
{
    internal ValueChangeEventArgs(TValue previousValue, TValue currentValue)
    {
        PreviousValue = previousValue;
        CurrentValue = currentValue;
    }

    public TValue PreviousValue { get; }
    public TValue CurrentValue { get; }
}
