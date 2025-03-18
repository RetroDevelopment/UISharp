using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;

namespace RetroDev.UISharp.Presentation.Properties;

// TODO: implement IDisposable

/// <summary>
/// A property wrapper that allows for dynamic one or two way binding.
/// </summary>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <remarks>
/// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
/// </remarks>
/// <remarks>
/// To access a <see cref="UIProperty{TValue}"/> value, use the <see cref="Value"/> property.
/// There is no implicit case implemented to avoid confusion between the following
/// <c>new UIProperty(AnotherProperty.Value, ...)</c>c>
/// and
/// <c>new UIProperty(AnotherProperty, ...)</c>
/// The former creates a property with an initial value, the latter creates a property bound to <c>AnotherProperty</c>.
/// </remarks>
[DebuggerDisplay("{Value}")]
public class UIProperty<TValue>
{
    private readonly bool _lockSetter;
    private readonly BehaviorSubject<TValue> _valueChangeSubject;

    private TValue _value;
    private IBinder? _binder;

    /// <summary>
    /// Allows to be notified whenever the <see cref="Value"/> property changes.
    /// </summary>
    public IObservable<TValue> ValueChange { get; }

    /// <summary>
    /// The application owing <see langword="this" /> <see cref="UIProperty{TValue}"/>.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The property value.
    /// </summary>
    public virtual TValue Value
    {
        set
        {
            ThrowIfSetNotAllowed();

            if (!EqualityComparer<TValue>.Default.Equals(_value, value))
            {
                _value = value;
                _valueChangeSubject.OnNext(value);
            }
        }
        get
        {
            Application?.Dispatcher?.ThrowIfNotOnUIThread();
            return _value;
        }
    }

    /// <summary>
    /// The allowed <see cref="BindingType"/>.
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed,
    /// including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </summary>
    public BindingType AllowedBinding { get; }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="application">The application owing <see langword="this" /> <see cref="UIProperty{TValue}"/>.</param>
    /// <param name="value">The property value.</param>
    /// <param name="allowedBinding">
    /// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
    /// </param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UIProperty{TValue}"/> during event handling.</param>
    public UIProperty(Application application, TValue value, BindingType allowedBinding = BindingType.TwoWays, bool lockSetter = false)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();
        _valueChangeSubject = new BehaviorSubject<TValue>(value);
        ValueChange = _valueChangeSubject.AsObservable();
        Application = application;
        _value = value;
        AllowedBinding = allowedBinding;
        _lockSetter = lockSetter;
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="application">The application owning this property.</param>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UIProperty{TValue}"/> during event handling.</param>
    public UIProperty(Application application, UIProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, BindingType allowedBinding = BindingType.TwoWays, bool lockSetter = false) : this(application, destinationProperty.Value, allowedBinding, lockSetter)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The <see cref="UIComponent"/> owning this property.</param>
    /// <param name="value">The property value.</param>
    /// <param name="allowedBinding">
    /// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
    /// </param>
    /// <remarks>
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </remarks>
    public UIProperty(UIComponent parent, TValue value, BindingType allowedBinding = BindingType.TwoWays) : this(parent.Application, value, allowedBinding, lockSetter: true)
    {
        ValueChange.Subscribe(v => parent.Invalidate());
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The <see cref="UIComponent"/> owning this property.</param>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    public UIProperty(UIComponent parent, UIProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, BindingType allowedBinding = BindingType.TwoWays) : this(parent, destinationProperty.Value, allowedBinding)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The <see cref="UIShape"/>> owning this property.</param>
    /// <param name="application">The application in which this property is running.</param>
    /// <param name="value">The property value.</param>
    /// <param name="allowedBinding">
    /// The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).
    /// </param>
    /// <remarks>
    /// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.SourceToDestination"/> and <see cref="BindingType.DestinationToSource"/>).
    /// </remarks>
    public UIProperty(UIShape parent, Application application, TValue value, BindingType allowedBinding = BindingType.TwoWays) : this(application, value, allowedBinding)
    {
        ValueChange.Subscribe(v => parent.Invalidate());
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The <see cref="UIShape"/> owning this property.</param>
    /// <param name="application">The application in which this property is running.</param>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="allowedBinding">The allowed <see cref="BindingType"/> (<see cref="BindingType.TwoWays"/> by default).</param>
    public UIProperty(UIShape parent, Application application, UIProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, BindingType allowedBinding = BindingType.TwoWays) : this(parent, application, destinationProperty.Value, allowedBinding)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> and removes every existing binding.
    /// </summary>
    /// <typeparam name="TDestination">The <paramref name="destinationProperty"/> value type.</typeparam>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public void Bind<TDestination>(UIProperty<TDestination> destinationProperty, BindingType bindingType, IBindingValueConverter<TValue, TDestination> converter)
    {
        ThrowIfSetNotAllowed();
        _binder?.Unbind();
        _binder = new PropertyBinder<TValue, TDestination>(this, destinationProperty, bindingType, converter);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void Bind(UIProperty<TValue> destinationProperty, BindingType bindingType)
    {
        Bind(destinationProperty, bindingType, ValueConverterFactory.Identity<TValue>());
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> and removes every existing binding.
    /// </summary>
    /// <typeparam name="TDestinationValueType">The <paramref name="destinationProperty"/> value type.</typeparam>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public void Bind<TDestinationValueType>(UIProperty<TDestinationValueType> destinationProperty,
                                            BindingType bindingType,
                                            Func<TValue, TDestinationValueType> sourceToDestinationConverter,
                                            Func<TDestinationValueType, TValue> destinationToSourceConverter)
    {
        Bind(destinationProperty, bindingType, ValueConverterFactory.FromLambda(sourceToDestinationConverter, destinationToSourceConverter));
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindSourceToDestination(UIProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.SourceToDestination);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TDestinationValueType">The <paramref name="destinationProperty"/> value type.</typeparam>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    public void BindSourceToDestination<TDestinationValueType>(UIProperty<TDestinationValueType> destinationProperty,
                                                               Func<TValue, TDestinationValueType> sourceToDestinationConverter)
    {
        Bind(destinationProperty, BindingType.SourceToDestination, sourceToDestinationConverter, _ => throw new InvalidOperationException());
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindDestinationToSource(UIProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.DestinationToSource);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TDestinationValueType">The <paramref name="destinationProperty"/> value type.</typeparam>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public void BindDestinationToSource<TDestinationValueType>(UIProperty<TDestinationValueType> destinationProperty,
                                                                    Func<TDestinationValueType, TValue> destinationToSourceConverter)
    {
        Bind(destinationProperty, BindingType.DestinationToSource, _ => throw new InvalidOperationException(), destinationToSourceConverter);
    }


    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="destinationProperty" /> is the destination property.
    /// </param>
    public void BindTwoWays(UIProperty<TValue> destinationProperty)
    {
        Bind(destinationProperty, BindingType.TwoWays);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="destinationProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TDestinationValueType">The <paramref name="destinationProperty"/> value type.</typeparam>
    /// <param name="destinationProperty">The destination property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public void BindTwoWays<TDestinationValueType>(UIProperty<TDestinationValueType> destinationProperty,
                                                   Func<TValue, TDestinationValueType> sourceToDestinationConverter,
                                                   Func<TDestinationValueType, TValue> destinationToSourceConverter)
    {
        Bind(destinationProperty, BindingType.TwoWays, sourceToDestinationConverter, destinationToSourceConverter);
    }

    /// <summary>
    /// Removes a binding if any.
    /// </summary>
    public void RemoveBinding()
    {
        Application?.Dispatcher.ThrowIfNotOnUIThread();
        _binder?.Unbind();
    }

    private void ThrowIfSetNotAllowed()
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        if (_lockSetter) Application.LifeCycle.ThrowIfPropertyCannotBeSet();
    }
}
