using RetroDev.UISharp;
using RetroDev.UISharp.Components.Collections;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// A pop-up menu with a list of items to select from.
/// </summary>
public class FlatMenu : UIOverlay
{
    private readonly ListBox _menuContent;

    /// <summary>
    /// Creates a new flat menu.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public FlatMenu(Application application) : base(application, CreateMenuContent(application))
    {
        _menuContent = (ListBox)Control;
    }

    /// <inheritdoc />
    protected internal override void AttachOwner(UIObject owner)
    {
    }

    /// <inheritdoc />
    protected internal override void DetachOwner(UIObject owner)
    {
    }

    private static ListBox CreateMenuContent(Application application)
    {
        var menuContent = new ListBox(application);
        menuContent.AutoWidth.Value = AutoSize.Wrap;
        menuContent.AutoHeight.Value = AutoSize.Wrap;
        menuContent.MaximumWidth.Value = 100;
        menuContent.MaximumHeight.Value = 100;
        return menuContent;
    }
}
