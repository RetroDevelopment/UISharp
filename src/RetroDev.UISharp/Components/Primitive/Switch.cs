using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Core.Shapes;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Themes;

namespace RetroDev.UISharp.Components.Simple;

/// <summary>
/// A toggle button switch to mark with a tick if something is checked.
/// </summary>
public class Switch : UIControl
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Circle _selectionCircle;

    /// <summary>
    /// Whether the switch is checked.
    /// </summary>
    public UIProperty<bool> Checked { get; }

    /// <summary>
    /// The color of the check box circle.
    /// </summary>
    public UIProperty<Color> CircleColor { get; }

    /// <summary>
    /// The background color of the switch when it <see cref="Switch"/> is <see langword="false" />.
    /// </summary>
    public UIProperty<Color> UncheckedBackgroundColor { get; }

    /// <summary>
    /// The background color when the switch is disabled.
    /// </summary>
    public UIProperty<Color> DisabledBackgroundColor { get; }

    /// <summary>
    /// The color indicating that the switch is focused.
    /// </summary>
    public UIProperty<Color> FocusColor { get; }

    /// <summary>
    /// The switch border color.
    /// </summary>
    public UIProperty<Color> BorderColor { get; }

    /// <summary>
    /// Creates a new switch toggle button.
    /// </summary>
    /// <param name="application">The application that contain <see langword="this" /> <see cref="Switch"/>.</param>
    /// <param name="checked">Whether the initial state is checked.</param>
    public Switch(Application application, bool @checked = false) : base(application, autoWidth: AutoSize.Wrap, autoHeight: AutoSize.Wrap)
    {
        Checked = new UIProperty<bool>(this, @checked);
        CircleColor = CreateNewColorPropertyFor<Switch>(UISharpColorNames.SwitchCircleColor);
        UncheckedBackgroundColor = CreateNewColorPropertyFor<Switch>(UISharpColorNames.SwitchOffBackground);
        DisabledBackgroundColor = CreateNewColorPropertyFor<Switch>(UISharpColorNames.SwitchDisabled);
        FocusColor = CreateNewColorPropertyFor<Switch>(UISharpColorNames.SwitchFocusBorder);
        BorderColor = CreateNewColorPropertyFor<Switch>(UISharpColorNames.SwitchBorder);

        BackgroundColor.BindTheme(UISharpColorNames.SwitchOnBackground);

        Padding.SetAll(5.0f); // TODO: use styles

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BorderThickness.Value = 3.0f; // TODO: use styles
        Canvas.Shapes.Add(_backgroundRectangle);

        _selectionCircle = new Circle(application);
        _selectionCircle.BackgroundColor.BindSourceToDestination(CircleColor);
        Canvas.Shapes.Add(_selectionCircle);

        MousePress += Switch_MousePress;
        RenderFrame += Switch_RenderFrame;
    }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
    {
        var size = Application.DefaultFont.Value.Size;
        return new Size(size * 3, size);
    }

    private void Switch_MousePress(UIObject sender, MouseEventArgs e)
    {
        if (e.Button != MouseButton.Left) return;
        Checked.Value = !Checked.Value;
        Focus.Value = true;
    }

    private void Switch_RenderFrame(UIObject sender, RenderingEventArgs e)
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
            _backgroundRectangle.BorderColor.Value = FocusColor.Value;
        }
        else
        {
            _backgroundRectangle.BorderColor.Value = BorderColor.Value;
        }
    }

    private void UpdateSelectionCirclePosition(RenderingEventArgs args)
    {
        var padding = Padding.ToMarginStruct();
        List<PixelUnit> paddings = [padding.Top.IfAuto(PixelUnit.Min),
                                    padding.Right.IfAuto(PixelUnit.Min),
                                    padding.Bottom.IfAuto(PixelUnit.Min),
                                    padding.Left.IfAuto(PixelUnit.Min)];
        var maxPadding = padding.IsAuto ? PixelUnit.Zero : paddings.Max()!;

        var height = args.RenderingAreaSize.Height - maxPadding * 2.0f;
        var width = height;
        var size = new Size(width, height);
        var area = Area.Empty;

        if (Checked.Value)
        {
            area = size.PositionCenterRightOf(args.RenderingAreaSize);
            area = new Area(new Point(area.TopLeft.X - padding.Right, area.TopLeft.Y), area.Size);
        }
        else
        {
            area = size.PositionCenterLeftOf(args.RenderingAreaSize);
            area = new Area(new Point(area.TopLeft.X + padding.Left, area.TopLeft.Y), area.Size);
        }

        _selectionCircle.RelativeRenderingArea.Value = area;
    }
}
