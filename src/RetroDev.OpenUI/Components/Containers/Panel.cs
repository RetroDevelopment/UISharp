﻿using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Core.Graphics.Coordinates;

namespace RetroDev.OpenUI.Components.Containers;

/// <summary>
/// A basic container that contains one object.
/// </summary>
public class Panel : UIContainer, ISingleContainer // TODO: ISingleContainer should have a property not SetComponent() and GetChildren does not apply
{
    private UIWidget? _child;

    /// <inheritdoc />
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) =>
        childrenSize.FirstOrDefault() ?? Size.Zero;

    /// <inheritdoc />

    public override IEnumerable<UIWidget> Children => [GetChildrenNodes().First()];

    /// <summary>
    /// Creates a new panel.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public Panel(Application application) : base(application)
    {
    }

    /// <summary>
    /// Sets the component to be inserted in <see langword="this" /> panel.
    /// </summary>
    /// <param name="component">The component to be inserted in <see langword="this" /> panel.</param>
    public void SetComponent(UIWidget component)
    {
        if (_child != null) RemoveChildNode(_child);
        _child = component;
        AddChildNode(_child);
    }
}
