using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Core;

namespace RetroDev.OpenIDE;

class MyComponent : Component
{
    private int width;
    private int height;

    public MyComponent(Component parent) : base(parent) { }

    protected override void Render(Canvas canvas)
    {
        if (!canvas.TextureExists("bg"))
        {
            canvas.CreateTexture("bg", new RgbaImage([100, 100, 100, 100], 1, 1));
        }

        if (!canvas.TextureExists("x"))
        {
            canvas.CreateTexture("x", new RgbaImage([255, 255, 255, 255,
                                                255, 0, 0, 255,
                                                0, 255, 0, 255,
                                                0, 0, 255, 255], 2, 2));
        }

        if (!canvas.TextureExists("img"))
        {
            var svg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"><circle cx=\"50\" cy=\"50\" r=\"40\" stroke=\"white\" stroke-width=\"2\" fill=\"red\" /></svg>";
            canvas.CreateSvgTexture("img", svg);
        }

        if (!canvas.TextureExists("txt"))
        {
            (this.width, this.height) = canvas.CreateTextTexture("txt", "Hello World!", 20);
        }

        canvas.RenderTexture("bg", new(new(0, 0), new(Width, Height)));
        canvas.RenderTexture("x", new(new(50, 50), new(100, 100)));
        canvas.RenderTexture("img", new(new(100, 100), new(50, 50)));
        canvas.RenderTexture("txt", new(new(0, 0), new(width, height)));
    }
}

internal class Program
{
    static void Main(string[] _)
    {
        var application = new Application();
        var window = new Window(application);
        var component = new MyComponent(window);
        window.X = 10;
        window.Y = 30;
        window.Width = 800;
        window.Height = 600;
        component.X = 50;
        component.Y = 20;
        component.Width = 400;
        component.Height = 200;
        window.Visible = true;
        component.Visible = true;

        // Assuming `window` is an instance of your Window class
        component.MousePress += (sender, args) =>
            Console.WriteLine($"MousePress: Button = {args.Button}, Position = ({args.AbsoluteLocation.X}, {args.AbsoluteLocation.Y})");

        component.MouseRelease += (sender, args) =>
            Console.WriteLine($"MouseRelease: Button = {args.Button}, Position = ({args.AbsoluteLocation.X}, {args.AbsoluteLocation.Y})");

        component.MouseMove += (sender, args) =>
            Console.WriteLine($"MouseMove: Position = ({args.AbsoluteLocation.X}, {args.AbsoluteLocation.Y})");

        component.KeyPress += (sender, args) =>
            Console.WriteLine($"KeyPress: Key = {args.Button}");

        component.KeyRelease += (sender, args) =>
            Console.WriteLine($"KeyRelease: Key = {args.Button}");

        component.KeyPress += (sender, args) => component.X++;

        application.Run();
    }
}
