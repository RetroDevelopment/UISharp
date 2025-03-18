using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

// TODO: implement IDisposable

/// <summary>
/// A property wrapper that allows for dynamic one or two way binding.
/// </summary>
/// <typeparam name="TValue">The property value type.</typeparam>
/// <remarks>
/// If <paramref name="allowedBinding"/> is <see cref="BindingType.TwoWays"/> it means that bidirectional binding is allowed, including (<see cref="BindingType.DestinationToSource"/> and <see cref="BindingType.SourceToDestination"/>).
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
    /// Whether <see langword="this" /> <see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </summary>
    /// <remarks>
    /// Set this property to <see langword="false" /> to avoid unexpected binding updates. This is useful for example in theme properties,
    /// where updates of a bound color (for example a button background) must not update the theme. In this example, given button.Background.Bind(theme.SomeColor, DestinationToSource)
    /// would not be allowed and <see cref="UIPropertyValidationException"/> will be thrown.
    /// </remarks>
    public bool CanReceiveBindingUpdates { get; }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="application">The application owing <see langword="this" /> <see cref="UIProperty{TValue}"/>.</param>
    /// <param name="value">The property value.</param>
    /// <param name="canReceiveBindingUpdates">
    /// Whether<see langword = "this" /> < see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UIProperty{TValue}"/> during event handling.</param>
    public UIProperty(Application application, TValue value, bool canReceiveBindingUpdates = true, bool lockSetter = false)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();
        _valueChangeSubject = new BehaviorSubject<TValue>(value);
        ValueChange = _valueChangeSubject.AsObservable();
        Application = application;
        _value = value;
        CanReceiveBindingUpdates = canReceiveBindingUpdates;
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
    /// <param name="canReceiveBindingUpdates">
    /// Whether<see langword = "this" /> < see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </param>
    /// <param name="lockSetter ">Whether it is only possible to set <see langword="this" /> <see cref="UIProperty{TValue}"/> during event handling.</param>
    public UIProperty(Application application, UIProperty<TValue> destinationProperty, BindingType bindingType, bool canReceiveBindingUpdates = true, bool lockSetter = false) : this(application, destinationProperty.Value, canReceiveBindingUpdates, lockSetter)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The <see cref="UIComponent"/> owning this property.</param>
    /// <param name="value">The property value.</param>
    /// <param name="canReceiveBindingUpdates">
    /// Whether<see langword = "this" /> < see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </param>
    public UIProperty(UIComponent parent, TValue value, bool canReceiveBindingUpdates = true) : this(parent.Application, value, canReceiveBindingUpdates, lockSetter: true)
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
    /// <param name="canReceiveBindingUpdates">
    /// Whether<see langword = "this" /> < see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </param>
    public UIProperty(UIComponent parent, UIProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, bool canReceiveBindingUpdates = true) : this(parent, destinationProperty.Value, canReceiveBindingUpdates)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Creates a new property.
    /// </summary>
    /// <param name="parent">The <see cref="UIShape"/>> owning this property.</param>
    /// <param name="application">The application in which this property is running.</param>
    /// <param name="value">The property value.</param>
    /// <param name="canReceiveBindingUpdates">
    /// Whether<see langword = "this" /> < see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </param>
    public UIProperty(UIShape parent, Application application, TValue value, bool canReceiveBindingUpdates = true) : this(application, value, canReceiveBindingUpdates)
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
    /// <param name="canReceiveBindingUpdates">
    /// Whether<see langword = "this" /> < see cref="UIProperty{TValue}"/> can receive binding updates, meaning that
    /// if this is the binding destination, <see cref="BindingType.SourceToDestination"/> is allowed, if it is the binding
    /// source, <see cref="BindingType.DestinationToSource"/> is allowed. If <see langword="false" /> the mentioned bindings will result in
    /// a <see cref="UIPropertyValidationException"/>.
    /// </param>
    public UIProperty(UIShape parent, Application application, UIProperty<TValue> destinationProperty, BindingType bindingType = BindingType.TwoWays, bool canReceiveBindingUpdates = true) : this(parent, application, destinationProperty.Value, canReceiveBindingUpdates)
    {
        Bind(destinationProperty, bindingType);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The destination property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the binding source property.
    /// </param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public void Bind<TSource>(UIProperty<TSource> sourceProperty, BindingType bindingType, IBindingValueConverter<TSource, TValue> converter)
    {
        ThrowIfSetNotAllowed();
        _binder?.Unbind();
        _binder = new PropertyBinder<TSource, TValue>(sourceProperty, this, bindingType, converter);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public void Bind(UIProperty<TValue> sourceProperty, BindingType bindingType)
    {
        Bind(sourceProperty, bindingType, ValueConverterFactory.Identity<TValue>());
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public void Bind<TSource>(UIProperty<TSource> sourceProperty,
                              BindingType bindingType,
                              Func<TSource, TValue> sourceToDestinationConverter,
                              Func<TValue, TSource> destinationToSourceConverter)
    {
        Bind(sourceProperty, bindingType, ValueConverterFactory.FromLambda(sourceToDestinationConverter, destinationToSourceConverter));
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public void BindDestinationToSource(UIProperty<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.DestinationToSource);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to source property value.</param>
    public void BindDestinationToSource<TSource>(UIProperty<TSource> sourceProperty,
                                                 Func<TSource, TValue> sourceToDestinationConverter)
    {
        Bind(sourceProperty, BindingType.DestinationToSource, sourceToDestinationConverter, _ => throw new InvalidOperationException());
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public void BindSourceToDestination(UIProperty<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.SourceToDestination);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to source property value.</param>
    public void BindSourceToDestination<TSource>(UIProperty<TSource> sourceProperty,
                                                 Func<TSource, TValue> sourceToDestinationConverter)
    {
        Bind(sourceProperty, BindingType.SourceToDestination, ValueConverterFactory.FromLambda(sourceToDestinationConverter));
    }


    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the destination property.
    /// </param>
    public void BindTwoWays(UIProperty<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.TwoWays);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.TwoWays"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The destination property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to destination property value.</param>
    /// <param name="destinationToSourceConverter">The function converting from destination property value to source property value.</param>
    public void BindTwoWays<TSource>(UIProperty<TSource> sourceProperty,
                                     Func<TSource, TValue> sourceToDestinationConverter,
                                     Func<TValue, TSource> destinationToSourceConverter)
    {
        Bind(sourceProperty, BindingType.TwoWays, sourceToDestinationConverter, destinationToSourceConverter);
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
