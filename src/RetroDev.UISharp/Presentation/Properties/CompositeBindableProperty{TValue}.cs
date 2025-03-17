using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// A property that contains multiple <see cref="UIProperty{TValue}"/>.
/// </summary>
/// <typeparam name="TValue">
/// The properties value type.
/// All <see cref="UIProperty{TValue}"/> value type must be equal to<typeparamref name="TValue"/>.
/// </typeparam>
public abstract class CompositeBindableProperty<TValue>
{
    private readonly Application _application;
    private readonly IEnumerable<UIProperty<TValue>> _properties;

    /// <summary>
    /// Creates a new <see cref="CompositeBindableProperty{TValue}"/>.
    /// </summary>
    /// <param name="application">The applicaciont in which this property runs.</param>
    /// <exception cref="UIPropertyValidationException">
    /// If one or more <see cref="UIProperty{TValue}"/> value does not match <typeparamref name="TValue"/>.
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
    /// Sets all <see cref="UIProperty{TValue}"/> values to the given <paramref name="value"/>.
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

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <exception cref="UIPropertyValidationException">
    /// If binding is invalid. That happens when source and destination composite properties do not have the same properties
    /// with the same name.
    /// </exception>
    public void Bind(CompositeBindableProperty<TValue> destinationProperty, BindingType bindingType)
    {
        var sourceProperties = GetType().GetBindableProperties();
        var destinationProperties = destinationProperty.GetType().GetBindableProperties();

        if (sourceProperties.Count != destinationProperties.Count)
        {
            throw new UIPropertyValidationException($"Composite property binding requires source and destination property to have the same number of bindable properties, but source property has {sourceProperties.Count} properties, destination property has {destinationProperties.Count} properties.");
        }

        var count = sourceProperties.Count;

        for (int i = 0; i < count; i++)
        {
            var source = sourceProperties[i];
            var destination = destinationProperties[i];
            if (source.Name != destination.Name)
            {
                throw new UIPropertyValidationException($"Composite property binding requires source and destination properties to have the same property names in the same order. Source property {source.PropertyType.Name} name missmatch with {destination.PropertyType.Name}");
            }

            source.GetBindableProperty<TValue>(this).Bind(destination.GetBindableProperty<TValue>(destinationProperty), bindingType);
        }
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindDestinationToSource(CompositeBindableProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.DestinationToSource);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindSourceToDestination(CompositeBindableProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.SourceToDestination);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindTwoWays(CompositeBindableProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.TwoWays);
    }
}
