using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Core;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Core.Events;
using RetroDev.OpenUI.Core.Internal;

namespace RetroDev.OpenUI;

/// <summary>
/// Displays and manage a window.
/// </summary>
public class Window : Component
{
    private readonly Application _parentApplication;

    public Window(Application parent, IWindowManager? windowManager = null) : base(parent.LifeCycle,
                                                                                   windowManager ?? new SDLWindowManager(parent.LifeCycle),
                                                                                   parent._svgEngine,
                                                                                   parent._fontEngine)
    {
        _parentApplication = parent;
        parent.AddWindow(this);
        VisibilityChange += (_, _) => _windowManager.Visible = Visible;
        parent._eventSystem.MousePress += EventSystem_MousePress;
        parent._eventSystem.MouseRelease += EventSystem_MouseRelease;
        parent._eventSystem.MouseMove += EventSystem_MouseMove;
        parent._eventSystem.KeyPress += EventSystem_KeyPress;
        parent._eventSystem.KeyRelease += EventSystem_KeyRelease;
    }

    public void Shutdown()
    {
        _parentApplication._eventSystem.MousePress -= EventSystem_MousePress;
        _renderingEngine.Shutdown();
    }

    protected internal override void Render()
    {
        _renderingEngine.InitializeFrame();
        base.Render();
        _renderingEngine.FinalizeFrame();
    }

    protected override void Render(Canvas canvas)
    {
    }

    private void EventSystem_MousePress(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnMousePress(this, windowArgs.Args);
        }
    }

    private void EventSystem_MouseRelease(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnMouseRelease(this, windowArgs.Args);
        }

    }

    private void EventSystem_MouseMove(object? sender, WindowEventArgs<MouseEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnMouseMove(this, windowArgs.Args);
        }
    }

    private void EventSystem_KeyPress(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnKeyPress(this, windowArgs.Args);
        }
    }

    private void EventSystem_KeyRelease(object? sender, WindowEventArgs<KeyEventArgs> windowArgs)
    {
        if (windowArgs.WindowId.Equals(_windowManager.WindowId))
        {
            OnKeyRelease(this, windowArgs.Args);
        }
    }
}
