using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Coordinates;

namespace RetroDev.UISharp.Components.Containers;

/// <summary>
/// A basic container that contains one object.
/// </summary>
public class Panel : UISingleContainer
{
    private bool _disableRemoval = false;

    /// <summary>
    /// Creates a new panel.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="component">The component to be inserted in <see langword="this" /> <see cref="Panel"/>.</param>
    public Panel(Application application, UIControl? component = null) : base(application)
    {
        Item.Value = component;
        Item.ValueChange.Subscribe(OnChildChange);
        Children.ValueRemove.Subscribe(_ =>
        {
            _disableRemoval = true;
            Item.Value = null;
            _disableRemoval = false;
        });
    }

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        if (Item.Value is not null)
        {
            return childrenSize.First().Inflate(Item.Value.Margin.ToMarginStruct());
        }

        return Size.Zero;
    }

    private void OnChildChange(UIControl? child)
    {
        if (_disableRemoval) return;
        Children.Clear();
        if (child is not null) Children.Add(child);
    }
}
