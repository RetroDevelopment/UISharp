﻿using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Presentation.Properties;
using RetroDev.OpenUI.Presentation.Properties.Exceptions;

namespace RetroDev.OpenUI.Components.Simple;

/// <summary>
/// A bar displaying progress.
/// </summary>
public class ProgressBar : UIWidget
{
    private readonly Rectangle _backgroundRectangle;
    private readonly Rectangle _progressRectangle;

    /// <summary>
    /// The current progress value.
    /// The progress bar will be filled on the percentage that <see cref="Value"/> is with respect to <see cref="MinimumValue"/> and <see cref="MaximumValue"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">If <see cref="Value"/> is not in the [<see cref="MinimumValue"/>, <see cref="MaximumValue"/>] interval.</exception>
    public UIProperty<ProgressBar, int> Value { get; }

    /// <summary>
    /// The minimum allowed <see cref="Value"/>.
    /// </summary>
    public UIProperty<ProgressBar, int> MinimumValue { get; }

    /// <summary>
    /// The current progress <see cref="Value"/>.
    /// </summary>
    public UIProperty<ProgressBar, int> MaximumValue { get; }

    /// <summary>
    /// The color of the bar indicating progress.
    /// </summary>
    public UIProperty<ProgressBar, Color> ForegroundColor { get; }

    /// <inheritdoc/>
    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(100, 20); // TODO: 20 is the common text size, 100 is some value to be big enough. Make sure that the size fits the screen.

    /// <summary>
    /// Creates a new label.
    /// </summary>
    /// <param name="application">The application that contain this progress bar.</param>
    public ProgressBar(Application application) : base(application, isFocusable: false)
    {
        Value = new UIProperty<ProgressBar, int>(this, 0);
        MinimumValue = new UIProperty<ProgressBar, int>(this, 0);
        MaximumValue = new UIProperty<ProgressBar, int>(this, 100);
        ForegroundColor = new UIProperty<ProgressBar, Color>(this, application.Theme.SecondaryColorDisabled, BindingType.DestinationToSource);
        BackgroundColor.BindDestinationToSource(Application.Theme.PrimaryColor);

        _backgroundRectangle = new Rectangle(application);
        _backgroundRectangle.BackgroundColor.BindDestinationToSource(BackgroundColor);
        Canvas.Add(_backgroundRectangle);

        _progressRectangle = new Rectangle(application);
        _progressRectangle.BackgroundColor.BindDestinationToSource(ForegroundColor);
        Canvas.Add(_progressRectangle);

        RenderFrame += ProgressBar_RenderFrame;
    }

    /// <inheritdoc />
    public override void Validate()
    {
        base.Validate();
        if (Value.Value < MinimumValue.Value) throw new UIPropertyValidationException($"Value {Value.Value} must be greater or equal to MinimumValue {MinimumValue.Value}", this);
        if (Value.Value > MaximumValue.Value) throw new UIPropertyValidationException($"Value {Value.Value} must be less than or equal to MaximumValue {MaximumValue.Value}", this);
        if (MaximumValue.Value < MinimumValue.Value) throw new UIPropertyValidationException($"MaximumValue {MaximumValue.Value} must be greater or equal to {MinimumValue.Value}", this);
    }

    protected override List<Area?> RepositionChildren(Size availableSpace, IEnumerable<Size> childrenSize)
    {
        var value = Math.Clamp(Value.Value, MinimumValue.Value, MaximumValue.Value);
        var percentage = (value - MinimumValue.Value) / (float)(MaximumValue.Value - MinimumValue.Value);
        var progressRectangleWidth = availableSpace.Width * percentage;
        return [null, new Area(Point.Zero, new Size(progressRectangleWidth, availableSpace.Height))];
    }

    private void ProgressBar_RenderFrame(UIComponent sender, OpenUI.Core.Windowing.Events.RenderingEventArgs e)
    {
        _backgroundRectangle.RelativeRenderingArea.Value = e.RenderingAreaSize.Fill();
        var value = Math.Clamp(Value.Value, MinimumValue.Value, MaximumValue.Value);
        var percentage = (value - MinimumValue.Value) / (float)(MaximumValue.Value - MinimumValue.Value);
        var progressRectangleWidth = e.RenderingAreaSize.Width * percentage;
        var progressRecangleSize = new Size(progressRectangleWidth, e.RenderingAreaSize.Height);
        _progressRectangle.RelativeRenderingArea.Value = progressRecangleSize.FillCenterLeftOf(e.RenderingAreaSize);
    }
}
