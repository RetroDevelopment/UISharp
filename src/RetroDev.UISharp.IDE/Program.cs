using System.Runtime;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using RetroDev.UISharp.Components.Collections;
using RetroDev.UISharp.Components.Core.AutoArea;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Layouts;
using RetroDev.UISharp.Components.Simple;
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
        application.ApplicationStarted += (_, _) => LoadTest(application);
        application.Run();
    }

    private static void LoadMain(Application application)
    {
        application.ShowWindow<MainWindow>();
    }

    private static void LoadTest(Application application)
    {
        var window = new Window(application);
        window.Width.Value = 800;
        window.Height.Value = 600;
        var tree = new TreeBox(application);
        tree.Width.Value = 200;
        tree.Height.Value = 200;
        window.Items.Add(tree);

        var t = new Component("Root", [], Create(4, 3, "C"));
        var vm = t.ToUIPropertyHierarchy(application, n => n.Components);
        tree.Items.BindSourceToDestination(vm, new Converter(tree));

        var add = new Button(application, "Add");
        var remove = new Button(application, "Remove");
        var collapseAll = new Button(application, "Collapse all");
        var expandAll = new Button(application, "Expand all");
        var modify = new Button(application, "Modify");

        add.X.Value = 0;
        add.Y.Value = 0;
        add.AutoWidth.Value = AutoSize.Wrap;
        add.AutoHeight.Value = AutoSize.Wrap;

        remove.X.Value = 0;
        remove.Y.Value = 40;
        remove.AutoWidth.Value = AutoSize.Wrap;
        remove.AutoHeight.Value = AutoSize.Wrap;

        collapseAll.X.Value = 0;
        collapseAll.Y.Value = 80;
        collapseAll.AutoWidth.Value = AutoSize.Wrap;
        collapseAll.AutoHeight.Value = AutoSize.Wrap;

        expandAll.X.Value = 0;
        expandAll.Y.Value = 110;
        expandAll.AutoWidth.Value = AutoSize.Wrap;
        expandAll.AutoHeight.Value = AutoSize.Wrap;

        modify.X.Value = 100;
        modify.Y.Value = 0;
        modify.AutoWidth.Value = AutoSize.Wrap;
        modify.AutoHeight.Value = AutoSize.Wrap;

        int seqNumber = 0;
        add.Action += (_, _) =>
        {
            var c = new Component("Synt " + seqNumber++, [], []);
            map[tree.SelectedNode.Value].Children.Add(new UITreeNode<Component>(application, c));
        };

        remove.Action += (_, _) =>
        {
            var it = map[tree.SelectedNode.Value];
            if (it.Parent is not null) it.Parent.Children.Remove(it);
            else vm.Children.Remove(it);
        };

        collapseAll.Action += (_, _) => vm.CollapseAll();
        expandAll.Action += (_, _) => vm.ExpandAll();
        modify.Action += (_, _) =>
        {
            var it = map[tree.SelectedNode.Value];
            it.Content.Value = new Component("Modified", [], []);
        };

        window.Items.Add(add);
        window.Items.Add(remove);
        window.Items.Add(expandAll);
        window.Items.Add(collapseAll);
        window.Items.Add(modify);
        window.Show();
    }

    private static List<Component> Create(int width, int depth, string label)
    {
        if (depth == 0) return [];

        var children = new List<Component>();
        for (int i = 0; i < width; i++)
        {
            var cl = $"{label}.{i}";
            children.Add(new Component(cl, [], Create(width, depth - 1, cl)));
        }

        return children;
    }

    private static Dictionary<UITreeNode<UIWidget>, UITreeNode<Component>> map = [];

    private class Converter(UIWidget owner) : IHierarchicalBindingValueConverter<Component, UIWidget>
    {
        private UIWidget _owner = owner;

        public Component ConvertDestinationToSource(UIWidget destination)
        {
            throw new NotImplementedException();
        }

        public UITreeNode<Component> ConvertDestinationToSource(UITreeNode<UIWidget> destination)
        {
            throw new NotImplementedException();
        }

        public UIWidget ConvertSourceToDestination(Component source)
        {
            int height = 20;
            if (source.Name.EndsWith("1")) height += 20;
            if (source.Name.EndsWith("2")) height += 35;

            var gl = new GridLayout(_owner.Application, 1, 2);
            var btn = new Button(_owner.Application, "Check");
            var lbl = new Label(_owner.Application, $"{source.Name}");
            lbl.Height.Value = height;
            gl.Items.Add(btn);
            gl.Items.Add(lbl);
            gl.AutoWidth.Value = AutoSize.Wrap;
            gl.HorizontalAlignment.Value = Alignment.Left;
            return gl;
        }

        public UITreeNode<UIWidget> ConvertSourceToDestination(UITreeNode<Component> source)
        {
            var nodeW = new UITreeNode<UIWidget>(_owner, ConvertSourceToDestination(source.Content.Value));
            map.Add(nodeW, source);
            return nodeW;
        }
    }
}
