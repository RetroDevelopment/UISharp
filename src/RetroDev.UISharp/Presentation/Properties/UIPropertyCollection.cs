using System.Collections;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Components.Shapes;

namespace RetroDev.UISharp.Presentation.Properties;

/// <summary>
/// Wraps a collection providing collection change notifications and bindings.
/// </summary>
/// <typeparam name="TValue">The collection elements value type.</typeparam>
public class UIPropertyCollection<TValue> : IList<TValue>
{
    private readonly bool _lockChanges;
    private readonly List<TValue> _values = [];
    private readonly Subject<int> _valueAddSubject;
    private readonly Subject<int> _valueRemoveSubject;
    private readonly Subject<int> _valueChangeSubject;

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
                _values[index] = value;
                _valueChangeSubject.OnNext(index);
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
    /// Notifies when a value is changed in the list an sends the zero-based index at which the item is located in the collection.
    /// </summary>
    public IObservable<int> ValueChange { get; }

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
        _valueChangeSubject = new Subject<int>();

        ValueAdd = _valueAddSubject.AsObservable();
        ValueRemove = _valueRemoveSubject.AsObservable();
        ValueChange = _valueChangeSubject.AsObservable();
        _lockChanges = lockChanges;
        IsReadOnly = false;
    }

    /// <summary>
    /// Creates a new collection.
    /// </summary>
    /// <param name="component">The <see cref="UIComponent"/> owning <see langword="this" /> <see cref="UIPropertyCollection{TValue}"/>.</param>
    /// <param name="lockChanges ">Whether it is only possible to change the collection state during event handling.</param>
    public UIPropertyCollection(UIComponent component, bool lockChanges = true) : this(component.Application, lockChanges)
    {
        ValueAdd.Subscribe(_ => component.Invalidate());
        ValueRemove.Subscribe(_ => component.Invalidate());
        ValueChange.Subscribe(_ => component.Invalidate());
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
        ValueChange.Subscribe(_ => shape.Invalidate());
    }

    /// <inheritdoc />
    public void Add(TValue item)
    {
        ThrowIfChangesNotAllowed();
        _values.Add(item);
        _valueAddSubject.OnNext(_values.Count - 1);
    }

    /// <inheritdoc />
    public void Clear()
    {
        ThrowIfChangesNotAllowed();
        var size = Count;
        _values.Clear();
        for (var i = 0; i < size; i++)
        {
            _valueRemoveSubject.OnNext(i);
        }
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
        var removed = _values.Remove(item);
        if (index >= 0)
        {
            _valueRemoveSubject.OnNext(index);
        }

        return removed;
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        ThrowIfChangesNotAllowed();
        _values.RemoveAt(index);
        _valueRemoveSubject.OnNext(index);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    private void ThrowIfChangesNotAllowed()
    {
        Application.Dispatcher.ThrowIfNotOnUIThread();
        if (_lockChanges) Application.LifeCycle.ThrowIfPropertyCannotBeSet();
        if (IsReadOnly) throw new InvalidOperationException("Cannot modify read-only collections");
    }
}
