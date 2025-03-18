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
    private readonly List<TValue> _values = [];
    private readonly Subject<int> _valueAdd;
    private readonly Subject<int> _valueRemove;
    private readonly Subject<int> _valueChange;

    /// <inheritdoc />
    public TValue this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    /// <inheritdoc />
    public int Count => throw new NotImplementedException();

    /// <inheritdoc />
    public bool IsReadOnly => throw new NotImplementedException();

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
    public UIPropertyCollection(Application application)
    {
        application.Dispatcher.ThrowIfNotOnUIThread();
        Application = application;
        _valueAdd = new Subject<int>();
        _valueRemove = new Subject<int>();
        _valueChange = new Subject<int>();

        ValueAdd = _valueAdd.AsObservable();
        ValueRemove = _valueRemove.AsObservable();
        ValueChange = _valueChange.AsObservable();
    }

    public UIPropertyCollection(UIComponent component) : this(component.Application)
    {
        ValueAdd.Subscribe(_ => component.Invalidate());
        ValueRemove.Subscribe(_ => component.Invalidate());
        ValueChange.Subscribe(_ => component.Invalidate());
    }

    public UIPropertyCollection(UIShape shape, Application application) : this(application)
    {
        ValueAdd.Subscribe(_ => shape.Invalidate());
        ValueRemove.Subscribe(_ => shape.Invalidate());
        ValueChange.Subscribe(_ => shape.Invalidate());
    }

    public void Add(TValue item)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(TValue item)
    {
        throw new NotImplementedException();
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public IEnumerator<TValue> GetEnumerator()
    {
        throw new NotImplementedException();
    }

    public int IndexOf(TValue item)
    {
        throw new NotImplementedException();
    }

    public void Insert(int index, TValue item)
    {
        throw new NotImplementedException();
    }

    public bool Remove(TValue item)
    {
        throw new NotImplementedException();
    }

    public void RemoveAt(int index)
    {
        throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        throw new NotImplementedException();
    }
}
