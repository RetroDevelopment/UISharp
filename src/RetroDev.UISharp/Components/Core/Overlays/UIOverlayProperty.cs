using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Core.Overlays;

/// <summary>
/// A <see cref="UIProperty{TValue}"/> for overlays that automatically manages overlay life-cycle.
/// </summary>
/// <typeparam name="TOverlay">The overlay type.</typeparam>
public class UIOverlayProperty<TOverlay> : UIProperty<TOverlay?> where TOverlay : UIOverlay
{
    private readonly UIObject _owner;

    /// <inheritdoc />
    public override TOverlay? Value
    {
        get => base.Value;
        set
        {
            var previousValue = base.Value;
            base.Value = value;

            if (previousValue is not null && value is null)
            {
                _owner.Overlays.Remove(previousValue);
            }

            if (value is not null)
            {
                _owner.Overlays.Add(value);
            }
        }
    }

    /// <summary>
    /// Creates a new overlay property.
    /// </summary>
    /// <param name="owner">The <see cref="UIObject" /> that owns the <see cref="UIOverlay"/> set as <see cref="Value"/>.</param>
    public UIOverlayProperty(UIObject owner) : base(owner, (TOverlay?)null)
    {
        _owner = owner;
    }
}
