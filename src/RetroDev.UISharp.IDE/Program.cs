using System.Data;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using RetroDev.UISharp.Components.Collections;
using RetroDev.UISharp.Components.Containers;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;
using RetroDev.UISharp.Components.Simple;
using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.IDE.Windows;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.UIDefinition.Ast;
using RetroDev.UISharp.Windows;

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
        var window = new Window(application);
        window.Show();
    }
}
