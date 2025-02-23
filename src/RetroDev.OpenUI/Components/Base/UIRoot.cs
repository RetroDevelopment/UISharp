using RetroDev.OpenUI.Components.Core;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Core.Contexts;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.OpenGL;
using RetroDev.OpenUI.Core.Windowing.SDL;

namespace RetroDev.OpenUI.Components.Base;

/// <summary>
/// The root of a UI component hierarchy. This is typically a rendering unit, such as, a window or a mobile
/// device activity.
/// </summary>
public abstract class UIRoot : UIComponent, IContainer
{
    /// <summary>
    /// The object that manages <see cref="UIComponent"/> invalidation for the component tree rooted by <see langword="this" />
    /// component.
    /// </summary>
    protected internal Invalidator Invalidator { get; }

    /// <summary>
    /// The object that measures all <see cref="UIComponent"/> measure calculation for the component tree rooted by <see langword="this" />
    /// component.
    /// </summary>

    protected MeasureProvider MeasureProvider { get; }

    /// <summary>
    /// The object that performes retained mode rendering of all <see cref="UIComponent"/> components in the component tree rooted by <see langword="this" />
    /// component.
    /// </summary>
    protected RenderProvider RenderProvider { get; }

    /// <summary>
    /// The engine that render into <see langword="this" /> root component.
    /// </summary>
    protected internal IRenderingEngine RenderingEngine { get; }

    public abstract IEnumerable<UIWidget> Children { get; }

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
    protected UIRoot(Application application,
                     IRenderingEngine? renderingEngine = null,
                     ComponentVisibility visibility = ComponentVisibility.Visible,
                     bool isFocusable = true,
                     IAutoSize? autoWidth = null,
                     IAutoSize? autoHeight = null,
                     IHorizontalAlignment? horizontalAlignment = null,
                     IVerticalAlignment? verticalAlignment = null) : base(application, visibility, isFocusable, autoWidth, autoHeight, horizontalAlignment, verticalAlignment)
    {
        Invalidator = new Invalidator();
        MeasureProvider = new MeasureProvider(Invalidator);
        RenderProvider = new RenderProvider(Invalidator);
        RenderingEngine = renderingEngine ?? new OpenGLRenderingEngine(application.Dispatcher, application.Logger, new SDLOpenGLRenderingContext(application));
    }

    /// <summary>
    /// Adds a <see cref="UIWidget"/> component to <see langword="this" /> root component.
    /// </summary>
    /// <param name="component">The component to add.</param>
    public void AddComponent(UIWidget component)
    {
        AddChildNode(component);
    }

    /// <summary>
    /// Gets the child component with <see cref="ID"/> equal to the given <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="TComponent">The comnponent type.</typeparam>
    /// <returns>The component.</returns>
    /// <exception cref="ArgumentException">If the component does not exist.</exception>
    /// <exception cref="InvalidCastException">If the component was found but with a type not assignable to <typeparamref name="TComponent"/>.</exception>
    public TComponent GetComponent<TComponent>(string id) where TComponent : UIWidget
    {
        var children = GetChildrenNodes().Where(c => c.ID.Value == id);
        if (!children.Any()) throw new ArgumentException($"Child with ID {id} not found in component with id {ID.Value}");
        return (TComponent)children.First();
    }
}
