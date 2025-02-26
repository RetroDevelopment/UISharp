using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.Coordinates;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A checkbox to mark with a tick if something is checked.
/// </summary>
public class CheckBox : UIWidget
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
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(80, 30); // TODO: Maybe same size as default label text size (which is 20).

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
        _backgroundRectangle.BorderColor.BindDestinationToSource(FocusColor);
        Canvas.Add(_backgroundRectangle);

        _selectionCircle = new Circle(application);
        _selectionCircle.BackgroundColor.BindDestinationToSource(CircleColor);
        Canvas.Add(_selectionCircle);

        MousePress += CheckBox_MousePress;
        RenderFrame += CheckBox_RenderFrame;
    }

    private void CheckBox_MousePress(UIComponent sender, MouseEventArgs e)
    {
        Checked.Value = !Checked.Value;
        Focus.Value = true;
    }

    private void CheckBox_RenderFrame(UIComponent sender, RenderingEventArgs e)
    {
        var cornerRadius = _backgroundRectangle.ComputeCornerRadius(1.0f, e.RenderingAreaSize);

        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
        _backgroundRectangle.CornerRadiusX.Value = cornerRadius;
        _backgroundRectangle.CornerRadiusY.Value = cornerRadius;
        UpdateBackgroundRectangleBorder();
        UpdateBackgroundRectangleColor();
        UpdateSelectionCirclePosition(e);
    }

    private void UpdateBackgroundRectangleColor()
    {
        if (!Enabled.Value)
        {
            _backgroundRectangle.BackgroundColor.Value = DisabledBackgroundColor.Value;
        }
        else if (Checked.Value)
        {
            _backgroundRectangle.BackgroundColor.Value = BackgroundColor.Value;
        }
        else
        {
            _backgroundRectangle.BackgroundColor.Value = UncheckedBackgroundColor.Value;
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

    private void UpdateSelectionCirclePosition(RenderingEventArgs args)
    {
        var height = args.RenderingAreaSize.Height;
        var width = height;
        var size = new Size(width, height);

        if (Checked.Value)
        {
            _selectionCircle.RelativeRenderingArea.Value = size.FillCenterRightOf(args.RenderingAreaSize);
        }
        else
        {
            _selectionCircle.RelativeRenderingArea.Value = size.FillCenterLeftOf(args.RenderingAreaSize);
        }
    }
}
