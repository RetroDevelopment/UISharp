using System.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Components.Core.Shapes;
using RetroDev.UISharp.Presentation.Properties.Binding;
using RetroDev.UISharp.Presentation.Properties.Exceptions;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Wraps a collection providing collection change notifications and bindings.
/// </summary>
/// <typeparam name="TValue">The collection elements value type.</typeparam>
public class UIPropertyCollection<TValue> : IList<TValue>, IProperty
{
    private readonly bool _lockChanges;
    private readonly List<TValue> _values = [];
    private readonly Subject<int> _valueAddSubject;
    private readonly Subject<int> _valueRemoveSubject;
    private IDisposable? _flatBinder;

    private IDisposable? _binder;
    private bool _isBinding = false;

    private uint _openScopes = 0;
    /// <summary>
    /// Whether a binding update is in progress.
    /// </summary>
    public bool IsBinding => _isBinding;

    /// <inheritdoc />
    public TValue this[int index]
    {
        get
        {
            Application.Dispatcher.ThrowIfNotOnUIThread();
            return _values[index];
        }
        set
        {
            ThrowIfChangesNotAllowed();
            var currentValue = _values[index];
            if (!EqualityComparer<TValue>.Default.Equals(currentValue, value))
            {
                _valueRemoveSubject.OnNext(index);
                _values[index] = value;
                _valueAddSubject.OnNext(index);
            }
        }
    }

    /// <inheritdoc />
    public int Count
    {
        get
        {
            Application.Dispatcher.ThrowIfNotOnUIThread();
            return _values.Count;
        }
    }

    /// <summary>
    /// A value indicating whether <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/> is read only.
    /// If it is the case, a <see cref="InvalidOperationException"/> will be thrown if attempting to modify the collection.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// The application owning <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/>.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// Notifies when a value is added in the list an sends the zero-based index at which the item is located in the collection.
    /// </summary>
    public IObservable<int> ValueAdd { get; }

    /// <summary>
    /// Notifies when a value is removed from the list an sends the zero-based index at which the item is located in the collection.
    /// </summary>
    public IObservable<int> ValueRemove { get; }

    /// <summary>
    /// Whether <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/> can receive binding updates, meaning that
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
    /// Whether it is the target of a binding, e.g., the source of a <see cref="BindingType.DestinationToSource"/> or the destination of a <see cref="BindingType.SourceToDestination"/>.
    /// </summary>
    public bool IsBindingTarget { get; internal set; } = false;

    /// <summary>
    /// Creates a new collection.
    /// </summary>
    /// <param name="application">The application owning <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/>.</param>
    /// <param name="lockChanges ">Whether it is only possible to change the collection state during event handling.</param>
    public UIPropertyCollection(Application application, bool lockChanges = false)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();
        Application = application;
        _valueAddSubject = new Subject<int>();
        _valueRemoveSubject = new Subject<int>();

        ValueAdd = _valueAddSubject.AsObservable();
        ValueRemove = _valueRemoveSubject.AsObservable();
        _lockChanges = lockChanges;
        IsReadOnly = false;
        CanReceiveBindingUpdates = true;
        IsBindingTarget = false;
    }

    /// <summary>
    /// Creates a new collection.
    /// </summary>
    /// <param name="component">The <see cref="UIObject"/> owning <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/>.</param>
    /// <param name="lockChanges ">Whether it is only possible to change the collection state during event handling.</param>
    public UIPropertyCollection(UIObject component, bool lockChanges = true) : this(component.Application, lockChanges)
    {
        ValueAdd.Subscribe(_ => component.Invalidate());
        ValueRemove.Subscribe(_ => component.Invalidate());
    }

    /// <summary>
    /// Creates a new collection.
    /// </summary>
    /// <param name="shape">The <see cref="UIShape"/> owning <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/>.</param>
    /// <param name="application">The application owning <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/>./param>
    public UIPropertyCollection(UIShape shape, Application application) : this(application)
    {
        ValueAdd.Subscribe(_ => shape.Invalidate());
        ValueRemove.Subscribe(_ => shape.Invalidate());
    }

    /// <inheritdoc />
    public void Add(TValue item)

    {
        ThrowIfChangesNotAllowed();
        _values.Add(item);
        _valueAddSubject.OnNext(_values.Count - 1);
    }

    /// <summary>
    /// Adds all the given <paramref name="items"/> to <see langword="this" /> collection.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public void AddRange(IEnumerable<TValue> items)
    {
        foreach (var item in items)
        {
            Add(item);
        }
    }

    /// <summary>
    /// Inserts the given <paramref name="items"/> in <see langword="this" /> <see cref="List{T}"/> at the
    /// specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">The zero-based index at which the elements should be inserted.</param>
    /// <param name="items">The elements to insert.</param>
    public void InsertRange(int index, IEnumerable<TValue> items)
    {
        ThrowIfChangesNotAllowed();
        _values.InsertRange(index, items);
        for (var i = index; i < index + items.Count(); i++)
        {
            _valueAddSubject.OnNext(i);
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        ThrowIfChangesNotAllowed();
        var size = Count;
        for (var i = 0; i < size; i++)
        {
            _valueRemoveSubject.OnNext(0);
        }
        _values.Clear();
    }

    /// <inheritdoc />
    public bool Contains(TValue item)
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        return _values.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(TValue[] array, int arrayIndex)
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        _values.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public IEnumerator<TValue> GetEnumerator()
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        return _values.GetEnumerator();
    }

    /// <inheritdoc />
    public int IndexOf(TValue item)
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        return _values.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, TValue item)
    {
        ThrowIfChangesNotAllowed();
        _values.Insert(index, item);
        _valueAddSubject.OnNext(index);
    }

    /// <inheritdoc />
    public bool Remove(TValue item)
    {
        ThrowIfChangesNotAllowed();
        var index = _values.IndexOf(item);
        if (index >= 0)
        {
            _valueRemoveSubject.OnNext(index);
        }

        return _values.Remove(item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        ThrowIfChangesNotAllowed();
        _valueRemoveSubject.OnNext(index);
        _values.RemoveAt(index);
    }

    /// <summary>
    /// Removes a range of elements from <see langword="this" /> <see cref="List{T}"/>.
    /// </summary>
    /// <param name="start">The zero-base starting index of the range of elements to remove.</param>
    /// <param name="count">The number of elements to remove.</param>
    public void RemoveRange(int start, int count)
    {
        ThrowIfChangesNotAllowed();
        for (var i = start; i < start + count; i++)
        {
            _valueRemoveSubject.OnNext(start);
        }
        _values.RemoveRange(start, count);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

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
    public virtual void Bind<TSource>(UIPropertyCollection<TSource> sourceProperty, BindingType bindingType, IBindingValueConverter<TSource, TValue> converter)
    {
        using var _ = CreateBindingScope();
        ThrowIfChangesNotAllowed();
        _binder?.Dispose();
        _binder = new UIPropertyCollectionBinder<TSource, TValue>(sourceProperty, this, bindingType, converter);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="bindingType">
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public virtual void Bind(UIPropertyCollection<TValue> sourceProperty, BindingType bindingType)
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
    public virtual void Bind<TSource>(UIPropertyCollection<TSource> sourceProperty,
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
    public virtual void BindDestinationToSource(UIPropertyCollection<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.DestinationToSource);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.DestinationToSource"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to source property value.</param>
    public virtual void BindDestinationToSource<TSource>(UIPropertyCollection<TSource> sourceProperty,
                                                         Func<TValue, TSource> sourceToDestinationConverter)
    {
        Bind(sourceProperty, BindingType.DestinationToSource, _ => throw new InvalidOperationException(), sourceToDestinationConverter);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// The <see cref="BindingType"/> (<see langword="this"/> property is the source property and).
    /// the given <paramref name="sourceProperty" /> is the source property.
    /// </param>
    public virtual void BindSourceToDestination(UIPropertyCollection<TValue> sourceProperty)
    {
        Bind(sourceProperty, BindingType.SourceToDestination);
    }

    /// <summary>
    /// Binds <see langword="this" /> property to the given <paramref name="sourceProperty"/> using <see cref="BindingType.SourceToDestination"/> binding and removes every existing binding.
    /// </summary>
    /// <typeparam name="TSource">The <paramref name="sourceProperty"/> value type.</typeparam>
    /// <param name="sourceProperty">The source property to bind.</param>
    /// <param name="sourceToDestinationConverter">The function converting from source property value to source property value.</param>
    public virtual void BindSourceToDestination<TSource>(UIPropertyCollection<TSource> sourceProperty,
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
    public virtual void BindTwoWays(UIPropertyCollection<TValue> sourceProperty)
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
    public virtual void BindTwoWays<TSource>(UIPropertyCollection<TSource> sourceProperty,
                                     Func<TSource, TValue> sourceToDestinationConverter,
                                     Func<TValue, TSource> destinationToSourceConverter)
    {
        Bind(sourceProperty, BindingType.TwoWays, sourceToDestinationConverter, destinationToSourceConverter);
    }

    /// <summary>
    /// Binds the given <paramref name="sourceHierarchy"/> to the <see langword="this" /> collection by flattening a tree into a list
    /// using DFS visit (Depth First Search).
    /// </summary>
    /// <typeparam name="TSource">The tree content value type.</typeparam>
    /// <param name="sourceHierarchy">The source collection in which to flatten <see langword="this" /> <see cref="UIPropertyHierarchy{TValue}"/>.</param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public virtual UIHierarchyFlattenBinder<TSource, TValue> FlatBindSourceToDestination<TSource>(UIPropertyHierarchy<TSource> sourceHierarchy, IBindingValueConverter<UITreeNode<TSource>, TValue> converter)
    {
        Unbind();
        var binder = new UIHierarchyFlattenBinder<TSource, TValue>(sourceHierarchy, this, converter);
        _flatBinder = binder;
        return binder;
    }

    /// <summary>
    /// Binds the given <paramref name="sourceHierarchy"/> to the <see langword="this" /> collection by flattening a tree into a list
    /// using DFS visit (Depth First Search).
    /// </summary>
    /// <typeparam name="TSource">The tree content value type.</typeparam>
    /// <param name="sourceHierarchy">The source collection in which to flatten <see langword="this" /> <see cref="UIPropertyHierarchy{TValue}"/>.</param>
    /// <param name="converter">A converter to convert source and destination property so that they match.</param>
    public virtual UIHierarchyFlattenBinder<TSource, TValue> FlatBindSourceToDestination<TSource>(UIPropertyHierarchy<TSource> sourceHierarchy, Func<UITreeNode<TSource>, TValue> converter) =>
        FlatBindSourceToDestination(sourceHierarchy, ValueConverterFactory.FromLambda(sourceToDestination: converter));

    /// <summary>
    /// Removes a binding if any.
    /// </summary>
    /// <remarks>
    /// This method does NOT perform deep unbinding but only shallow unbinding.
    /// This means that it does NOT remove nested binding of properties made inside the bound object.
    /// </remarks>
    public virtual void Unbind()
    {
        using var _ = CreateBindingScope();
        ThrowIfChangesNotAllowed();
        _binder?.Dispose();
        _flatBinder?.Dispose();
    }

    /// <summary>
    /// Creates a new <see cref="IDisposable" /> that allows for binding operation (temporarily allowing collection changes even for read-only collection).
    /// </summary>
    /// <returns>A disposable scope.</returns>
    /// <example>
    /// When performing a binding operation <c>b()</c> requiring temporarily allowing read-only collections to be modified, do
    /// using (var scope = collection.CreateBindingScope) { ... }
    /// </example>
    public IDisposable CreateBindingScope() =>
        new BindingScope(this);

    private void ThrowIfChangesNotAllowed()
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        if (_lockChanges) Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        if (IsReadOnly && !_isBinding) throw new InvalidOperationException("Cannot modify read-only collections");
    }

    private record BindingScope : IDisposable
    {
        private readonly UIPropertyCollection<TValue> _collection;

        public BindingScope(UIPropertyCollection<TValue> collection)
        {
            _collection = collection;
            _collection._isBinding = true;
            _collection._openScopes++;
        }

        public void Dispose()
        {
            _collection._openScopes--;
            if (_collection._openScopes == 0) _collection._isBinding = false;
        }
    }
}
