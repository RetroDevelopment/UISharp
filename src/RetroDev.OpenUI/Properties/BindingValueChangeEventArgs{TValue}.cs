namespace RetroDev.OpenUI.Properties;

public class BinderValueChangeEventArgs<TValue> : EventArgs
{
    public TValue Value { get; }

    public BinderValueChangeEventArgs(TValue value)
    {
        Value = value;
    }
}
