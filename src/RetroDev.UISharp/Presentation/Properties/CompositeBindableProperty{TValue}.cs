using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// A property that contains multiple <see cref="BindableProperty{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">
/// The properties value type.
/// All <see cref="BindableProperty{TValue}"/> value type must be equal to<typeparamref name="TValue"/>.
/// </typeparam>
public abstract class CompositeBindableProperty<TValue>
{
    private readonly Application _application;
    private readonly IEnumerable<BindableProperty<TValue>> _properties;

    /// <summary>
    /// Creates a new <see cref="CompositeBindableProperty{TValue}"/>.
    /// </summary>
    /// <param name="application">The applicaciont in which this property runs.</param>
    /// <exception cref="UIPropertyValidationException">
    /// If one or more <see cref="BindableProperty{TValue}"/> value does not match <typeparamref name="TValue"/>.
    /// </exception>
    protected CompositeBindableProperty(Application application)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();
        _application = application;
        var propertyInfos = GetType().GetBindableProperties();

        if (propertyInfos.Any(p => p.GetBindingValueType() != typeof(TValue)))
        {
            throw new UIPropertyValidationException($"Composite property of type {typeof(TValue)} must only have bindable properties of that type");
        }

        _properties = propertyInfos.Select(p => p.GetBindableProperty<TValue>(this));
    }

    /// <summary>
    /// Sets all <see cref="BindableProperty{TValue}"/> values to the given <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The value to set.</param>
    public void SetAll(TValue value)
    {
        _application.Dispatcher.ThrowIfNotOnUIThread();
        foreach (var property in _properties)
        {
            property.Value = value;
        }
    }
}
