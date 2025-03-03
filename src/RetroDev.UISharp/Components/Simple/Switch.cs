using System.ComponentModel.DataAnnotations;
using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A toogle button switch to mark with a tick if something is checked.
/// </summary>
public class Switch : UIWidget
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Circle _selectionCircle;

    /// <summary>
    /// Whether the switch is checked.
    /// </summary>
    public UIProperty<Switch, bool> Checked { get; }

    /// <summary>
    /// The color of the check box circle.
    /// </summary>
    public UIProperty<Switch, Color> CircleColor { get; }

    /// <summary>
    /// The background color of the switch when it <see cref="Switch"/> is <see langword="false" />.
    /// </summary>
    public UIProperty<Switch, Color> UncheckedBackgroundColor { get; }

    /// <summary>
    /// The background color when the switch is disabled.
    /// </summary>
    public UIProperty<Switch, Color> DisabledBackgroundColor { get; }

    /// <summary>
    /// The color indicating that the switch is focused.
    /// </summary>
    public UIProperty<Switch, Color> FocusColor { get; }

    /// <summary>
    /// Creates a new switch toggle button.
    /// </summary>
    /// <param name="application">The application that contain <see langword="this" /> <see cref="Switch"/>.</param>
    /// <param name="checked">Whether the initial state is checked.</param>
    public Switch(Application application, bool @checked = false) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Checked = new UIProperty<Switch, bool>(this, @checked);
        CircleColor = new UIProperty<Switch, Color>(this, Application.Theme.TextColor, BindingType.DestinationToSource);
        UncheckedBackgroundColor = new UIProperty<Switch, Color>(this, Application.Theme.PrimaryColor, BindingType.DestinationToSource);
        DisabledBackgroundColor = new UIProperty<Switch, Color>(this, Application.Theme.PrimaryColorDisabled, BindingType.DestinationToSource);
        FocusColor = new UIProperty<Switch, Color>(this, Application.Theme.BorderColor, BindingType.DestinationToSource);

        BackgroundColor.BindDestinationToSource(Application.Theme.SecondaryColor);

        Padding.SetAll(5.0f); // TODO: use styles

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BorderColor.BindDestinationToSource(FocusColor);
        Canvas.Add(_backgroundRectangle);

        _selectionCircle = new Circle(application);
        _selectionCircle.BackgroundColor.BindDestinationToSource(CircleColor);
        Canvas.Add(_selectionCircle);

        MousePress += Switch_MousePress;
        RenderFrame += Switch_RenderFrame;
    }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        var size = Application.DefaultFont.Value.Size;
        return new Size(size * 3, size);
    }

    private void Switch_MousePress(UIComponent sender, MouseEventArgs e)
    {
        if (e.Button != MouseButton.Left) return;
        Checked.Value = !Checked.Value;
        Focus.Value = true;
    }

    private void Switch_RenderFrame(UIComponent sender, RenderingEventArgs e)
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
        var padding = Padding.ToMarginStruct();
        var maxPadding = Math.Max(padding.Top.IsAuto ? PixelUnit.Min : padding.Top,
                                  Math.Max(padding.Right.IsAuto ? PixelUnit.Min : padding.Right,
                                  Math.Max(padding.Bottom.IsAuto ? padding.Bottom : PixelUnit.Min,
                                           padding.Left.IsAuto ? padding.Left : PixelUnit.Min)));
        if (padding.IsAuto) maxPadding = PixelUnit.Zero;

        var height = args.RenderingAreaSize.Height - maxPadding * 2.0f;
        var width = height;
        var size = new Size(width, height);
        var area = Area.Empty;

        if (Checked.Value)
        {
            area = size.PositionCenterRightOf(args.RenderingAreaSize, padding);
            area = new Area(new Point(area.TopLeft.X, area.TopLeft.Y), area.Size);
        }
        else
        {
            area = size.PositionCenterLeftOf(args.RenderingAreaSize, padding);
            area = new Area(new Point(area.TopLeft.X, area.TopLeft.Y), area.Size);
        }

        _selectionCircle.RelativeRenderingArea.Value = area;
    }
}
