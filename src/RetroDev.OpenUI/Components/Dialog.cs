using RetroDev.OpenUI.Core.Graphics;

namespace RetroDev.OpenUI.Components;

/// <summary>
/// A dialog window that might return a value when closed.
/// </summary>
/// <typeparam name="TResult">The returned value type.</typeparam>
public class Dialog<TResult> : Window
{
    private TResult? _result;

    /// <summary>
    /// The dialog result if any, otherwise <see langword="null" />.
    /// The result can only be set internally in the deriving classes, preferably via <see cref="Close(TResult?)"/>.
    /// </summary>
    public TResult? Result
    {
        get
        {
            Application.LifeCycle.ThrowIfNotOnUIThread();
            return _result;
        }
        protected set
        {
            Application.LifeCycle.ThrowIfNotOnUIThread();
            _result = value;
        }
    }

    /// <summary>
    /// Creates a new dialog.
    /// </summary>
    /// <param name="application">The application owning this window.</param>
    /// <param name="renderingEngine">
    /// The rendering engine to render this window. By default the <see cref="OpenGLRenderingEngine"/> with <see cref="SDLOpenGLRenderingContext"/> is created.
    /// If you want to create another rendering engine (e.g. Vulkan) or you are not using the standard <see cref="SDLWindowManager"/>, you pass an instance of
    /// <see cref="IRenderingEngine"/>. If using opengl but not based on SDL, you can create a <see cref="OpenGLRenderingEngine"/> but you must implement and use your custom
    /// instance of <see cref="IRenderingContext"/>.
    /// </param>
    public Dialog(Application application, IRenderingEngine? renderingEngine = null) : base(application, renderingEngine)
    {
        Visibility.ValueChange += Visibility_ValueChange;
    }

    /// <summary>
    /// Shows <see langword="this" /> <see cref="Dialog{TResult}"/> and blocks the calling thread until the
    /// window is closed.
    /// </summary>
    /// <param name="owner">The modal owner, or <see langword="null" /> if the dialog has no owner.</param>
    /// <returns>The dialog result, or <see langword="null" /> if no result is provided.</returns>
    /// <remarks>
    /// There are subtle differences between various window show methods in oreder to allow for flexibility.
    /// <list type="bullet">
    ///     <item>
    ///         <see cref="Window.Show"/> diplays the window and proceeds with execution as normal without altering any other window behavior.
    ///     </item>
    ///     <item>
    ///         <see cref="Window.ShowModal(Window)"/> displays the window as modal of the given parent window <c>owner</c>.
    ///         If <see langword="this" /> window is modal of <c>owner</c> then every event directed to <c>owner</c> will be ignored except for rendering and custom events.
    ///         Events of other windows that are not part of any modal child hierarchy will still be intercepted.
    ///     </item>
    ///     <item>
    ///         <see cref="Dialog{TResult}.ShowDialog(Window?)"/> with <paramref name="owner"/> non <see langword="null"/> shows the dialog as modal (see point above)
    ///         it blocks the caller execution until the dialog is closed, after which it returns the value provided by <see cref="Result"/>.
    ///     </item>
    ///     <item>
    ///         <see cref="Dialog{TResult}.ShowDialog(Window?)"/> with <paramref name="owner"/> <see langword="null"/> shows the dialog as non modal
    ///         but it still blocks the caller execution until the dialog is closed, after which it returns the value provided by <see cref="Result"/>.
    ///     </item>
    /// </list>
    /// </remarks>
    public TResult? ShowDialog(Window? owner)
    {
        if (owner != null) ShowModal(owner);
        else Show();

        while (Visibility.Value == ComponentVisibility.Visible)
        {
            Application.LifeCycle.CurrentState = LifeCycle.State.EVENT_POLL;
            Application.EventSystem.ProcessEvents();
        }

        return Result;
    }

    /// <summary>
    /// Closes the dialog and sets the given result.
    /// </summary>
    /// <param name="result">The dialog result.</param>
    protected void Close(TResult? result)
    {
        Application.LifeCycle.ThrowIfNotOnUIThread();
        Result = result;
        Close();
    }

    private void Visibility_ValueChange(UI.Properties.BindableProperty<ComponentVisibility> sender, UI.Properties.ValueChangeEventArgs<ComponentVisibility> e)
    {
        if (Visibility.Value != ComponentVisibility.Visible)
        {
            Application.EventSystem.Quit(emitQuitEvent: false);
        }
    }
}
