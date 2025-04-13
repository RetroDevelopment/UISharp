using System.Reactive.Disposables;
using RetroDev.UISharp;
using RetroDev.UISharp.Components.Collections;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// A pop-up menu with a list of items to select from.
/// </summary>
public class FlatMenu : UIOverlay
{
    private readonly ListBox _menuContent;
    private readonly CompositeDisposable _menuDisposable = new();

    /// <summary>
    /// The menu items.
    /// </summary>
    public UIPropertyCollection<UIControl> Items { get; }

    /// <summary>
    /// The item selected in the menu.
    /// </summary>
    public UIProperty<UIControl?> SelectedItem { get; }

    /// <summary>
    /// Whether the context menu is visible.
    /// </summary>
    public UIProperty<bool> Visible { get; }

    /// <summary>
    /// The auto width strategy for the <see langword="this" /> <see cref="FlatMenu"/> items.
    /// </summary>
    public UIProperty<IAutoSize> ItemsAutoWidth { get; }

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
        Visible = new UIProperty<bool>(_menuContent, false);
        ItemsAutoWidth = new UIProperty<IAutoSize>(_menuContent, _menuContent.ItemsAutoWidth.Value);

        _menuContent.Items.BindSourceToDestination(Items);
        _menuContent.SelectedItem.BindTwoWays(SelectedItem);
        _menuContent.Visibility.BindSourceToDestination(Visible, visible => visible ? UIObject.ComponentVisibility.Visible : UIObject.ComponentVisibility.Hidden);
        _menuContent.ItemsAutoWidth.BindSourceToDestination(ItemsAutoWidth);
        _menuContent.ItemSelected += MenuContent_ItemSelected;
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
        _menuDisposable.Dispose();
    }

    private void Owner_RenderingAreaChange(UIObject sender, RenderingAreaEventArgs e)
    {
        Application.Dispatcher.Schedule(() => RepositionListBox(sender));
    }

    private void Owner_MousePress(UIObject sender, MouseEventArgs e)
    {
        if (e.Button != MouseButton.Left) return;
        RepositionListBox(sender);
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
        if (_menuContent.Surface is null) return;

        var surfaceHeight = _menuContent.Surface.ActualSize.Height;
        var heightTopOfOwner = owner.ActualAbsoluteLocation.Y;
        var heightBottomOfOwner = surfaceHeight - (owner.ActualAbsoluteLocation.Y + owner.ActualSize.Height);

        if (heightBottomOfOwner > heightTopOfOwner)
        {
            RepositionListBoxBottom(owner);
        }
        else
        {
            RepositionListBoxTop(owner);
        }
    }

    private void RepositionListBoxBottom(UIObject owner)
    {
        _menuContent.X.Value = owner.ActualAbsoluteLocation.X;
        _menuContent.Y.Value = owner.ActualAbsoluteLocation.Y + owner.ActualSize.Height;
        _menuContent.Width.Value = owner.ActualSize.Width;
    }

    private void RepositionListBoxTop(UIObject owner)
    {
        _menuContent.X.Value = owner.ActualAbsoluteLocation.X;
        _menuContent.Y.Value = owner.ActualAbsoluteLocation.Y - _menuContent.ActualSize.Height;
        _menuContent.Width.Value = owner.ActualSize.Width;
    }

    private void MenuContent_ItemSelected(ListBox sender, EventArgs e)
    {
        Visible.Value = false;
    }
}
