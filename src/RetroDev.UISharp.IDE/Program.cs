using System.Diagnostics;
using RetroDev.UISharp.Components;
using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Core.Windowing.Events;
using RetroDev.UISharp.IDE.Windows;
using static System.Net.Mime.MediaTypeNames;

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
        w.Width.Value = 800;
        w.Height.Value = 600;
        var gl = new GridLayout(application, 2, 2);
        gl.AutoWidth.Value = AutoSize.Wrap;
        //gl.AutoHeight.Value = AutoSize.Wrap;
        gl.Margin.Left.Value = 100;
        gl.HorizontalAlignment.Value = Alignment.Left;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                var btn = new Button(application, $"Click {i}-{j}");
                btn.Margin.Left.Value = 5;
                btn.Margin.Right.Value = 5;
                btn.Margin.Top.Value = 5;
                btn.Margin.Bottom.Value = 5;
                gl.AddComponent(btn);
            }
        }
        w.AddComponent(gl);
        w.Show();
    }
}
