using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Helpers;
using RetroDev.UISharp.Core.Contexts;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Graphics.OpenGL;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.Core.Windowing.SDL;

namespace RetroDev.UISharp.Components.Core.Base;

/// <summary>
/// The root of a UI component hierarchy. This is typically a rendering unit, such as, a window or a mobile
/// device activity.
/// </summary>
public abstract class UISurface : UIContainer
{
    private int? _overlayStartIndex = null;

    /// <summary>
    /// The object that manages <see cref="UIObject"/> invalidation for the component tree rooted by <see langword="this" />
    /// component.
    /// </summary>
    protected internal Invalidator Invalidator { get; }

    /// <summary>
    /// The object that measures all <see cref="UIObject"/> measure calculation for the component tree rooted by <see langword="this" />
    /// component.
    /// </summary>

    protected MeasureProvider MeasureProvider { get; }

    /// <summary>
    /// The object that performs retained mode rendering of all <see cref="UIObject"/> components in the component tree rooted by <see langword="this" />
    /// component.
    /// </summary>
    protected RenderProvider RenderProvider { get; }

    /// <summary>
    /// The engine that render into <see langword="this" /> root component.
    /// </summary>
    internal IRenderingEngine RenderingEngine { get; }

    /// <summary>
    /// Useful informations for event handling.
    /// </summary>
    internal GlobalEventInformation GlobalEventInformation { get; } = new GlobalEventInformation();

    /// <summary>
    /// Creates a new root component.
    /// </summary>
    /// <param name="application">The application owning this component.</param>
    /// <param name="renderingEngine">
    /// The rendering engine to render this root component. By default the <see cref="OpenGLRenderingEngine"/> with <see cref="SDLOpenGLRenderingContext"/> is created.
    /// If you want to create another rendering engine (e.g. Vulkan) or you are not using the standard <see cref="SDLWindowManager"/>, you pass an instance of
    /// </param>
    /// <param name="visibility">Whether the component is rendered or not.</param>
    /// <param name="isFocusable">Whether the component can get focus.</param>
    /// <param name="autoWidth">How to automatically determine this component width.</param>
    /// <param name="autoHeight">How to automatically determine this component height.</param>
    /// <param name="horizontalAlignment">The component horizontal alignment (relative to its <see cref="Parent"/>).</param>
    /// <param name="verticalAlignment">The component vertical alignment (relative to its <see cref="Parent"/>).</param>
    protected UISurface(Application application,
                     IRenderingEngine? renderingEngine = null,
                     ComponentVisibility visibility = ComponentVisibility.Visible,
                     bool isFocusable = true,
                     IAutoSize? autoWidth = null,
                     IAutoSize? autoHeight = null,
                     IHorizontalAlignment? horizontalAlignment = null,
                     IVerticalAlignment? verticalAlignment = null) : base(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment)
    {
        Invalidator = new Invalidator(application);
        MeasureProvider = new MeasureProvider(Invalidator);
        RenderProvider = new RenderProvider(Invalidator);
        RenderingEngine = renderingEngine ?? new OpenGLRenderingEngine(application.Dispatcher, application.Logger, new SDLOpenGLRenderingContext(application.Logger));

        Items.ValueAdd.Subscribe(OnItemAdd);
        Items.ValueRemove.Subscribe(OnItemRemove);

        MouseMove += UIRoot_MouseMove;
        MouseRelease += UIRoot_MouseRelease;
    }

    internal void EnsureZIndicesUpdated()
    {
        if (Invalidator.NeedZIndexUpdate)
        {
            UpdateZIndices(1);
            Invalidator.NeedZIndexUpdate = false;
        }
    }

    internal void AttachOverlay(UIOverlay overlay)
    {
        Children.Add(overlay.Control);

        if (_overlayStartIndex is null)
        {
            _overlayStartIndex = Children.Count - 1;
        }
    }

    internal void DetachOverlay(UIOverlay overlay)
    {
        var overlayIndex = Children.IndexOf(overlay.Control);
        if (overlayIndex == -1) throw new ArgumentException($"Failed to detach overlay {overlay}: cannot detach overlays that are not attached");
        Children.Remove(overlay.Control);
        if (overlayIndex == _overlayStartIndex)
        {
            if (overlayIndex + 1 < Children.Count) _overlayStartIndex++;
            else _overlayStartIndex = null;
        }
    }

    private void UIRoot_MouseMove(UIObject sender, MouseEventArgs e)
    {
        foreach (var node in GlobalEventInformation.DraggingComponents)
        {
            node.OnMouseDrag(node.CreateEventWithRelativeLocation(e));
        }
    }

    private void UIRoot_MouseRelease(UIObject sender, MouseEventArgs e)
    {
        if (e.Button == MouseButton.Left)
        {
            foreach (var node in GlobalEventInformation.DraggingComponents)
            {
                node.OnMouseDragEnd();
            }

            GlobalEventInformation.ClearDraggedComponents();
        }
    }

    private void OnItemAdd(int index)
    {
        var item = Items[index];
        Children.Insert(index, item);
    }

    private void OnItemRemove(int index)
    {
        Children.RemoveAt(index);
    }
}
