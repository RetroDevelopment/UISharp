using RetroDev.UISharp.Components;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.IDE.Windows;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.IDE;

//TODO
// O. Implement logic for checking loops (e.g. SizeHint does not call RelativeDrawingArea, AddChild() do not create loops, etc.)

// SELLING POINTS
// 1. OpenGL and svg vector graphics (efficient, high quality, support for 3d rendering)
// 2. Allow flexible UI (dynamically load xml UI)
// 3. Simplicity for medium size projects (easier than WPF)
// 4. Cross platofrm (Window, mac, linux)
// 5. Testable (create UI integration tests)

// Unlike traditional UI frameworks, this system is designed to strike a balance between high-performance OpenGL rendering and ease of use, targeting developers who need:
// Cross-platform UI development.
// Dynamic and scalable interfaces.
// Lightweight and efficient tools for medium-sized applications.

internal class Program
{
    static void Main(string[] _)
    {
        using var application = new Application();
        application.Logger.Verbosity = Verbosity.Verbose;
        application.ApplicationStarted += (_, _) => LoadMain(application);
        application.Run();
    }

    private static void LoadMain(Application application)
    {
        application.ShowWindow<MainWindow>();
    }

    private static void LoadTest(Application application)
    {
        Window w = new Window(application);
        w.X.ValueChange.Subscribe(_ => application.Logger.LogError("X = " + w.X.Value));
        w.Width.ValueChange.Subscribe(_ => application.Logger.LogError("WIDTH = " + w.Width.Value));
        w.MinimumWidth.Value = 100;
        w.MaximumWidth.Value = 400;
        w.MinimumHeight.Value = 100;
        w.MaximumHeight.Value = 300;
        var edit = new EditBox(application, "0123456789 0123456789 0123456789 0123456789");
        edit.CaretIndex.Value = 3;
        edit.SelectionLength.Value = 7;
        w.AddComponent(edit);

        var b = new Button(application, "Click");
        b.Y.Value = 100;
        b.Height.Value = 30;
        w.AddComponent(b);
        w.Title.Value = "Hello title!";
        b.Action += (_, _) => b.Text.Value = new MD(application, edit).ShowDialog(w).ToString();

        w.Show();
    }

    class MD : Dialog<string>
    {
        EditBox es;
        public MD(Application application, EditBox e, IRenderingEngine? renderingEngine = null) : base(application, renderingEngine)
        {
            CloseBehavior.Value = WindowCloseBehavior.None;
            WindowCloseRequest += MD_WindowCloseRequest;
            var b = new Button(application, "Done");
            b.Action += (_, _) => Close("<CLOSED>");
            b.Margin.SetAll(30);
            AddComponent(b);
            es = e;
        }

        private void MD_WindowCloseRequest(Window sender, EventArgs e)
        {
            Close(es.Text.Value);
        }
    }
}
