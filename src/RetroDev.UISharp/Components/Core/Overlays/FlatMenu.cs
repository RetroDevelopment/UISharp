using RetroDev.UISharp;
using RetroDev.UISharp.Components.Collections;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Core.Events;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// A pop-up menu with a list of items to select from.
/// </summary>
public class FlatMenu : UIOverlay
{
    private readonly ListBox _menuContent;

    /// <summary>
    /// The menu items.
    /// </summary>
    public UIPropertyCollection<UIControl> Items { get; }

    /// <summary>
    /// The item selected in the menu.
    /// </summary>
    public UIProperty<UIControl?> SelectedItem { get; }

    /// <summary>
    /// <summary>
    /// Creates a new flat menu.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    public FlatMenu(Application application) : base(application, CreateMenuContent(application))
    {
        _menuContent = (ListBox)Control;
        Items = new UIPropertyCollection<UIControl>(_menuContent);
        SelectedItem = new UIProperty<UIControl?>(_menuContent, (UIControl?)null);

        _menuContent.Items.BindSourceToDestination(Items);
        _menuContent.SelectedItem.BindTwoWays(SelectedItem);
    }

    /// <inheritdoc />
    protected internal override void AttachOwner(UIObject owner)
    {
        owner.RenderingAreaChange += Owner_RenderingAreaChange;
        owner.MousePress += Owner_MousePress;
    }

    /// <inheritdoc />
    protected internal override void DetachOwner(UIObject owner)
    {
        owner.RenderingAreaChange -= Owner_RenderingAreaChange;
        owner.MousePress -= Owner_MousePress;
    }

    private void Owner_RenderingAreaChange(UIObject sender, RenderingAreaEventArgs e)
    {
        Application.Dispatcher.Schedule(() => RepositionListBox(sender));
    }

    private void Owner_MousePress(UIObject sender, MouseEventArgs e)
    {
        if (e.Button != MouseButton.Left) return;
        RepositionListBox(sender);
        _menuContent.Visibility.Value = UIObject.ComponentVisibility.Visible;
    }

    private static ListBox CreateMenuContent(Application application)
    {
        var menuContent = new ListBox(application);
        menuContent.AutoWidth.Value = AutoSize.Wrap;
        menuContent.AutoHeight.Value = AutoSize.Wrap;
        menuContent.MaximumHeight.Value = 100;
        menuContent.Visibility.Value = UIObject.ComponentVisibility.Collapsed;
        return menuContent;
    }

    private void RepositionListBox(UIObject owner)
    {
        if (_menuContent is null) return;
        _menuContent.X.Value = owner.ActualAbsoluteLocation.X;
        _menuContent.Y.Value = owner.ActualAbsoluteLocation.Y + owner.ActualSize.Height;
        _menuContent.Width.Value = owner.ActualSize.Width;
    }
}
