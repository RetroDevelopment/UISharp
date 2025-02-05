using RetroDev.OpenUI.Events;

namespace RetroDev.OpenUI.Utils;

public class CachedValue<TValue>(Func<TValue> compute)
{
    private TValue? _value;
    private Func<TValue> _compute = compute;

    public event TypeSafeEventHandler<CachedValue<TValue>, EventArgs> OnMarkDirty = (_, _) => { };

    public TValue Value
    {
        get
        {
            _value ??= _compute();
            return _value;
        }
    }

    public TValue? PreviousValue { get; private set; }

    public TValue Recompute()
    {
        MarkDirty();
        return Value;
    }

    public void MarkDirty()
    {
        if (_value != null) PreviousValue = _value;
        _value = default;
        OnMarkDirty.Invoke(this, EventArgs.Empty);
    }
}
