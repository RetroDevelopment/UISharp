using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Properties;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A checkbox to mark with a tick if something is checked.
/// </summary>
public class CheckBox : UIComponent
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Circle _selectionCircle;

    /// <summary>
    /// Whether the checkbox is checked.
    /// </summary>
    public UIProperty<CheckBox, bool> Checked { get; }

    /// <summary>
    /// The color of the check box circle.
    /// </summary>
    public UIProperty<CheckBox, Color> CircleColor { get; }

    /// <summary>
    /// The background color of the checkbox when it <see cref="CheckBox"/> is <see langword="false" />.
    /// </summary>
    public UIProperty<CheckBox, Color> UncheckedBackgroundColor { get; }

    /// <summary>
    /// The background color when the checkbox is disabled.
    /// </summary>
    public UIProperty<CheckBox, Color> DisabledBackgroundColor { get; }

    /// <summary>
    /// The color indicating that the checkbox is focused.
    /// </summary>
    public UIProperty<CheckBox, Color> FocusColor { get; }

    /// <inheritdoc/>
    protected override Size ComputeSizeHint(IEnumerable<Size> childrenSize) => new(80, 30); // TODO: Maybe same size as default label text size (which is 20).

    /// <summary>
    /// Creates a new checkbox.
    /// </summary>
    /// <param name="application">The application that contain this checkbox.</param>
    public CheckBox(Application application) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Checked = new UIProperty<CheckBox, bool>(this, false);
        CircleColor = new UIProperty<CheckBox, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        UncheckedBackgroundColor = new UIProperty<CheckBox, Color>(this, Application.Theme.PrimaryColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<CheckBox, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<CheckBox, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);

        BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColor);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.AutoCornerRadiusRatio.Value = 1.0f;
        _backgroundRectangle.BorderColor.BindDestinationToSource(Application.Theme.BorderColor);
        UpdateBackgroundRectangleColorBindings();
        UpdateBackgroundRectangleBorder();
        AddChild(_backgroundRectangle);

        _selectionCircle = new Circle(application);
        _selectionCircle.BackgroundColor.BindDestinationToSource(CircleColor);
        UpdateSelectionCirclePosition();
        AddChild(_selectionCircle);

        Checked.ValueChange += Checked_ValueChange;
        Focus.ValueChange += Focus_ValueChange;
        Enabled.ValueChange += Enabled_ValueChange;
        MousePress += CheckBox_MousePress;
    }

    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        var location = new Point(PixelUnit.Auto, PixelUnit.Auto);
        var size = new Size(availableSpace.Height, availableSpace.Height);
        return [null, new Area(location, size)];
    }

    private void CheckBox_MousePress(UIComponent sender, Events.MouseEventArgs e)
    {
        Checked.Value = !Checked;
        Focus.Value = true;
    }

    private void Checked_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateBackgroundRectangleColorBindings();
        UpdateSelectionCirclePosition();
        UpdateBackgroundRectangleBorder();
    }

    private void Focus_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateBackgroundRectangleBorder();
    }

    private void Enabled_ValueChange(BindableProperty<bool> sender, ValueChangeEventArgs<bool> e)
    {
        UpdateBackgroundRectangleColorBindings();
    }

    private void UpdateBackgroundRectangleColorBindings()
    {
        if (!Enabled.Value)
        {
            _backgroundRectangle.BackgroundColor.BindDestinationToSource(DisabledBackgroundColor);
        }
        else if (Checked.Value)
        {
            _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        }
        else
        {
            _backgroundRectangle.BackgroundColor.BindDestinationToSource(UncheckedBackgroundColor);
        }
    }

    private void UpdateBackgroundRectangleBorder()
    {
        if (Focus.Value)
        {
            _backgroundRectangle.BorderThickness.Value = 5.0f;
        }
        else
        {
            _backgroundRectangle.BorderThickness.Value = 0.0f;
        }
    }

    private void UpdateSelectionCirclePosition()
    {
        if (Checked.Value)
        {
            _selectionCircle.HorizontalAlignment.Value = Alignment.Right;
        }
        else
        {
            _selectionCircle.HorizontalAlignment.Value = Alignment.Left;
        }
    }

}
