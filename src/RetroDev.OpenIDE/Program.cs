using System.Diagnostics;
using RetroDev.OpenIDE.Windows;
using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Components.Containers;
using RetroDev.OpenUI.Components.Core.AutoArea;
using RetroDev.OpenUI.Components.Shapes;
using RetroDev.OpenUI.Components.Simple;
using RetroDev.OpenUI.Core.Graphics;
using RetroDev.OpenUI.Core.Graphics.Coordinates;
using RetroDev.OpenUI.Core.Logging;
using RetroDev.OpenUI.Core.Windowing.Events;
using RetroDev.OpenUI.Presentation;

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

class Ctr : UIWidget
{
    static int idx = 0;
    private Rectangle _rectangle;
    private Circle _circle;

    public Ctr(Application application, Color color, Area? area) : base(application, isFocusable: true)
    {
        if (area != null)
        {
            X.Value = area.TopLeft.X;
            Y.Value = area.TopLeft.Y;
            Width.Value = area.Size.Width;
            Height.Value = area.Size.Height;
        }

        _rectangle = new Rectangle(application);
        _rectangle.BackgroundColor.Value = color;
        _rectangle.BorderColor.Value = Focus.Value ? Color.Red : Color.Transparent;
        _rectangle.BorderThickness.Value = 5;
        _rectangle.CornerRadiusX.Value = 10;
        _rectangle.CornerRadiusY.Value = 10;

        MousePress += (_, _) => Focus.Value = Focusable.Value;
        MouseDrag += (_, e) => { if (Focus.Value) { X.Value += e.Offset.X; Y.Value += e.Offset.Y; } };
        Focus.ValueChange += (_, _) => _rectangle.BorderColor.Value = Focus.Value ? Color.Red : Color.Transparent;
        KeyPress += (_, e) =>
        {
            if (e.Button == KeyButton.Q && Focus.Value) Width.Value += 10;
            if (e.Button == KeyButton.W && Focus.Value) Height.Value += 10;
            if (e.Button == KeyButton.A && Focus.Value) Width.Value -= 10;
            if (e.Button == KeyButton.S && Focus.Value) Height.Value -= 10;
            if (e.Button == KeyButton.R && Focus.Value) _rectangle.Rotation.Value += 0.01f;
        };
        AddChildNode(_rectangle);

        var c = new Circle(application);
        c.Width.Value = 10;
        c.Height.Value = 10;
        c.BackgroundColor.Value = Color.Brown;
        AddChildNode(c);

        var txt = new Text(application);
        txt.TextColor.Value = Color.Magenta;
        txt.DisplayText.Value = $"CTX {idx++}";
        txt.HorizontalAlignment.Value = Alignment.Center;
        txt.VerticalAlignment.Value = Alignment.Top;
        txt.Width.Value = 30;
        txt.Height.Value = 10;
        //AddChild(txt);
    }

    public void Add(Ctr child) => AddChildNode(child);

    protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize) => new(100, 100);
}

internal class Program
{
    static void Mainm(string[] _)
    {
        using var application = new Application();
        application.Logger.Verbosity = Verbosity.Verbose;

        var root = new EditBox(application);
        root.Font.Value = new Font(application, application.DefaultFont.Value.Name, 16, FontType.Regular);

        Window window = new Window(application);
        window.Width.Value = 800;
        window.Height.Value = 600;
        window.Visibility.Value = UIComponent.ComponentVisibility.Visible;
        window.AddComponent(root);

        application.Run();
    }

    [DebuggerDisplay("{label}")]
    class Node : UIComponent
    {
        public string label = "";
        public Size Hint = new(100, 100);

        public Node(Application a, string l) : base(a) { label = l; }

        public void Add(UIWidget c) => AddChildNode(c);
        protected override Size ComputeMinimumOptimalSize(IEnumerable<Size> childrenSize)
        {
            return new(childrenSize.ToList().Sum(c => c.Width.Value) + Hint.Width,
                       childrenSize.ToList().Sum(c => c.Height.Value) + Hint.Height);
        }
    }

    static void Mainj(string[] _)
    {
        using var application = new Application();
        application.Logger.Verbosity = Verbosity.Verbose;
        //Window w2 = new Window(application);
        //w2.X.Value = 0;
        //w2.Y.Value = 0;
        //w2.Width.Value = 800;
        //w2.Height.Value = 600;
        //w2.Visibility.Value = ComponentVisibility.Visible;
        //var a = new Node(application, "A");
        //var b = new Node(application, "B");
        //var c = new Node(application, "C");
        //var d = new Node(application, "D");
        //var e = new Node(application, "E");
        //b.Add(d);
        //b.Add(e);
        //w2.AddComponent(a);
        //w2.AddComponent(b);
        //w2.AddComponent(c);
        //w2.MouseMove += (_, _) =>
        //{
        //    d.Hint = new Size(d.Hint.Width + 100, d.Hint.Height);
        //    d.Invalidate();
        //};
        //application.Run();
        //return;

        Window w = new Window(application);
        w.X.Value = 0;
        w.Y.Value = 0;
        w.Width.Value = 800;
        w.Height.Value = 600;
        w.Visibility.Value = UIComponent.ComponentVisibility.Visible;

        var root = new Ctr(application, Color.Blue, null);
        w.AddComponent(root);
        root.Focusable.Value = false;

        var child1 = new Ctr(application, Color.Black, new(Point.Zero, new Size(300, 200)));
        root.Add(child1);
        var child2 = new Ctr(application, Color.Black, new(new Point(100, 400), new Size(100, 100)));
        root.Add(child2);
        var child3 = new Ctr(application, Color.Black, new(new Point(400, 200), new Size(200, 200)));
        root.Add(child3);

        var child11 = new Ctr(application, Color.Yellow, new(Point.Zero, new Size(40, 40)));
        var child12 = new Ctr(application, Color.Yellow, new(new Point(20, 20), new Size(60, 80)));
        child1.Add(child11);
        child1.Add(child12);
        w.Focus.Value = true;

        var H = 30;
        var margin = 10;
        for (int y = -999990; y <= 999990; y += H + margin)
        {
            child3.Add(new Ctr(application, Color.Mint, new Area(new Point(PixelUnit.Auto, y), new Size(50, H))));
        }

        application.Run();
    }

    static void Main(string[] _)
    {
        using var application = new Application();
        application.Logger.Verbosity = Verbosity.Verbose;
        application.ApplicationStarted += (_, _) => application.ShowWindow<MainWindow>();
        application.Run();
    }
}
