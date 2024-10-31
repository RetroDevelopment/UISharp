using System.Runtime.CompilerServices;
using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.Core.Internal;

/// <summary>
/// Manages the UI properties.
/// </summary>
/// <param name="owner">The property manager parent object.</param>
/// <param name="lifeCycle">The UI life cycle current state.</param>
internal class PropertyManager(LifeCycle lifeCycle, object owner)
{
    private readonly object _owner = owner;
    private readonly Dictionary<string, object?> _propertyMap = [];

    /// <summary>
    /// Sets a property and notifies the corresponding property change event.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="propertyValue">The property value to set.</param>
    /// <param name="eventCallback">The property change callback.</param>
    /// <param name="propertyName">The property name. Omitting this parameter in a setter will set it to the setter name.</param>
    /// <exception cref="InvalidUIThreadException">If invoking this method from a non UI thread.</exception>
    public void SetProperty<T>(T propertyValue, EventHandler eventCallback, [CallerMemberName] string propertyName = "")
    {
        lifeCycle.ThrowIfNotOnUIThread();
        lifeCycle.ThrowIfPropertyCannotBeSet();

        if (!_propertyMap.TryGetValue(propertyName, out var value) || !Equals(value, propertyValue))
        {
            _propertyMap[propertyName] = propertyValue;
            eventCallback?.Invoke(_owner, new EventArgs());
        }
    }

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="propertyName">The property name. Omitting this parameter in a setter will set it to the getter name.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="InvalidUIThreadException">If invoking this method from a non UI thread.</exception>
    public T? GetNullableProperty<T>([CallerMemberName] string propertyName = "")
    {
        lifeCycle.ThrowIfNotOnUIThread();
        if (!_propertyMap.TryGetValue(propertyName, out var value)) return default;
        return (T?)value;
    }

    /// <summary>
    /// Gets the property value.
    /// </summary>
    /// <typeparam name="T">The property value type.</typeparam>
    /// <param name="propertyName">The property name. Omitting this parameter in a setter will set it to the getter name.</param>
    /// <returns>The property value.</returns>
    /// <exception cref="InvalidUIThreadException">If invoking this method from a non UI thread.</exception>
    public T GetNonNullableProperty<T>(T defaultValue, [CallerMemberName] string propertyName = "")
    {
        lifeCycle.ThrowIfNotOnUIThread();
        if (!_propertyMap.TryGetValue(propertyName, out var value)) return defaultValue;
        return (T)value!;
    }
}
