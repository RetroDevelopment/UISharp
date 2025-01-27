using RetroDev.OpenUI;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Graphics;
using RetroDev.OpenUI.Graphics.Shapes;

namespace RetroDev.OpenIDE.Windows;

internal class RemoveMe : UIComponent
{
    private float radiusX = 0;
    private float radiusY = 0;
    private float rotation = 0;
    private float thickness = 0;

    public RemoveMe(Application application) : base(application)
    {
        RenderFrame += RemoveMe_RenderFrame;
    }

    private void RemoveMe_RenderFrame(UIComponent sender, OpenUI.Events.RenderingEventArgs e)
    {
        radiusX = (radiusX + 1) % 20.0f;
        radiusY = (radiusY + 1.5f) % 25.0f;
        rotation = (rotation + 0.1f) % (3.14f * 2);
        thickness = (thickness + 1) % 35.0f;

        var rec = new Rectangle(BackgroundColor: new Color(255, 0, 0, 255),
                                BorderColor: new Color(0, 255, 0, 255),
                                BorderThickness: thickness,
                                CornerRadiusX: radiusX,
                                CornerRadiusY: radiusY,
                                Rotation: rotation);
        e.Canvas.Render(rec, new Area(new Point(30.0f, 30.0f), new Size(80.0f, 90.0f)));
    }

    protected override Size ComputeSizeHint(IEnumerable<Size> childrenSize)
    {
        return new Size(100, 100);
    }
}
