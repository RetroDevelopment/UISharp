using OpenTK.Graphics.ES11;
using RetroDev.OpenIDE.Windows;
using RetroDev.OpenUI;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Themes;

namespace RetroDev.OpenIDE;

//TODO
// O. Implement logic for checking loops (e.g. SizeHint does not call RelativeDrawingArea, AddChild() do not create loops, etc.)

// O. Implement ContextMenu
// O. Implement layouts! GridLayout, VerticalLayout, HorizontalLayout, GridBagLayout, etc.
// 8. Implement DropDown
// 9. Implement margins in all compoentns
// O. Implement colors and theming
// O. Implement text resources and languages
// O. Implement font size and font selection from file
// ===
// O. Implement button logic (focus colors, 3 states clicking, etc.)
// O. Implement logging
// O. Implement complex focus logic (focus change on tab finding the next focusable object, and maybe focus groups).
// O. Implement dispatcher and syncrhonization context WPF
// => cleanup (e.g. remove svg converter, revisit interfaces: no need for skia interfaces?)

// 1a. Implement logging
// 2a. Implement look and feel
// 3a. C++ platform specifics (modal windows, convert keyboard to character based on window layout, special handling of window resizing for Windows 10/11)
// 4a. SVG to Opengl?
// 5a. Optimize! (texture atlas, instance rendering, retained mode with glScissor to update only part of the screen)
// 6a. Versioning and build pipelines?
// 7a. Validators? Maybe intercept valueChange of components to add validation logic. Something builtin in framework?
// 8a. Material design? Like add PBR and lighting.
// 9a. Logging

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
        // application.Logger.Verbosity = OpenUI.Logging.Verbosity.Debug;
        application.ShowWindow<MainWindow>();
        application.Run();
    }
}
