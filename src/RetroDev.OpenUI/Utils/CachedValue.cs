using RetroDev.OpenUI.Events;

namespace RetroDev.OpenUI.Utils;

/// <summary>
/// Wraps a cached value that can be re-computed on demand.
/// </summary>
/// <typeparam name="TValue">The cached value type.</typeparam>
/// <param name="compute">A function that computes the value to update to <see langword="this" /> cache.</param>
public class CachedValue<TValue>(Func<TValue> compute)
{
    private TValue? _value;
    private Func<TValue> _compute = compute;

    /// <summary>
    /// Raised when the cache value needs to be re-computed.
    /// </summary>
    public event TypeSafeEventHandler<CachedValue<TValue>, EventArgs> OnMarkDirty = (_, _) => { };

    /// <summary>
    /// The cached value.
    /// </summary>
    public TValue Value
    {
        get
        {
            _value ??= _compute();
            return _value;
        }
    }

    /// <summary>
    /// Mark <see langword="this"/> cache to be re-computed.
    /// The immadiate next access to <see cref="Value"/> will re-compute the value, but subsequent access will return the cached value.
    /// </summary>
    public void MarkDirty()
    {
        _value = default;
        OnMarkDirty.Invoke(this, EventArgs.Empty);
    }
}
