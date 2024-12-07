using RetroDev.OpenUI.Events;

namespace RetroDev.OpenUI.Utils;

public class CachedValue<T>(Func<T> compute)
{
    private T? _value;
    private Func<T> _compute = compute;

    public event TypeSafeEventHandler<CachedValue<T>, EventArgs> OnMarkDirty = (_, _) => { };

    public T Value
    {
        get
        {
            _value ??= _compute();
            return _value;
        }
    }

    public void MarkDirty()
    {
        _value = default;
        OnMarkDirty.Invoke(this, EventArgs.Empty);
    }
}
