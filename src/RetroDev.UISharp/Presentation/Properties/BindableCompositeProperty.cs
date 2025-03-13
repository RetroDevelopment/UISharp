namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// A property wrapper that group multiple <see cref="BindableProperty{TValue}"/> instances.
/// You can use this class for properties that are logically grouped.
/// </summary>
public abstract class BindableCompositeProperty<TValue>
{
    private readonly Application _application;

    protected BindableCompositeProperty(Application application)
    {
        _application = application;
    }

    public void SetAll(TValue value) { }
}
