using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Presentation.Properties;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;

namespace RetroDev.UISharp.Components.Collections.DropDownHelpers;

interface IPreviewRenderer
{
    TObject Clone<TObject>(TObject item) where TObject : UIObject;
}

/// <summary>
/// Clones <see cref="UIObject"/> components to be displayed as preview.
/// </summary>
public class UIObjectCloner(Application application) : IPreviewRenderer
{
    public Application Application { get; } = application;

    public virtual TObject Clone<TObject>(TObject item) where TObject : UIObject
    {
        var itemType = item.GetType();
        var clonedItem = itemType.CreateInstance<TObject>(Application);
        BindProperties(item, clonedItem);
        clonedItem.Enabled.Value = false; // Avoid interacting with the object causing state conflicts (e.g. focus lost).
        return clonedItem;
    }

    // TODO: just call Dispose()?
    public virtual void Unbind(UIObject item)
    {
        var itemType = item.GetType();
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        foreach (var itemPropertyTypeInfo in itemType.GetProperties(bindingFlags))
        {
            var itemPropertyInfo = itemType.GetProperty(itemPropertyTypeInfo.Name, bindingFlags);
            if (itemPropertyInfo is null || !itemPropertyInfo.PropertyType.IsAssignableTo(typeof(IProperty))) continue;
            var propertyValue = itemPropertyInfo.GetValue(item);
            if (propertyValue is null) continue;
            ((IProperty)propertyValue).Unbind();
            var itemPropertyType = propertyValue.GetType();

            var isUIObjectPropertyCollection =
                itemPropertyType.IsGenericType &&
                itemPropertyType.GetGenericTypeDefinition() == typeof(UIPropertyCollection<>) &&
                itemPropertyType.GetGenericArguments()[0].IsAssignableTo(typeof(UIObject));
            if (isUIObjectPropertyCollection)
            {
                var propertyCollection = (IEnumerable<UIObject>)propertyValue;
                foreach (var nestedItem in propertyCollection)
                {
                    Unbind(nestedItem);
                }
            }
        }
    }

    protected virtual void BindProperties<TObject>(TObject sourceItem, TObject destinationItem) where TObject : UIObject
    {
        var destType = destinationItem.GetType();
        var sourceType = sourceItem.GetType();
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        foreach (var destinationPropertyInfo in destType.GetProperties(bindingFlags))
        {
            var sourcePropertyInfo = sourceType.GetProperty(destinationPropertyInfo.Name, bindingFlags);
            if (sourcePropertyInfo is null) throw new ArgumentException($"Failed to match destination property {destinationPropertyInfo} with source property: source property is null");
            var sourcePropertyValue = sourcePropertyInfo.GetValue(sourceItem);
            var destinationPropertyValue = destinationPropertyInfo.GetValue(destinationItem);
            if (sourcePropertyValue is null || destinationPropertyValue is null)
                continue;

            BindProperty(sourcePropertyInfo, sourcePropertyValue, destinationPropertyInfo, destinationPropertyValue);
        }
    }

    protected virtual void BindProperty(PropertyInfo sourcePropertyInfo,
                                        object sourcePropertyValue,
                                        PropertyInfo destinationPropertyInfo,
                                        object destinationPropertyValue)
    {
        if (!sourcePropertyInfo.PropertyType.IsAssignableTo(typeof(IProperty)) ||
            !destinationPropertyInfo.PropertyType.IsAssignableTo(typeof(IProperty)))
        {
            return;
        }

        if (TryBindUIObjectPropertyCollectionRecursively(sourcePropertyInfo, sourcePropertyValue, destinationPropertyInfo, destinationPropertyValue)) return;

        var sourcePropertyType = sourcePropertyValue.GetType();
        var destPropertyType = destinationPropertyValue.GetType();

        // Regular BindSourceToDestination(sourceProperty)
        var bindMethod = destPropertyType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m =>
                m.Name == nameof(UIProperty<object>.BindSourceToDestination) &&
                m.GetParameters().Length == 1 &&
                m.GetParameters()[0].ParameterType == sourcePropertyType);

        if (bindMethod is not null)
        {
            bindMethod.Invoke(destinationPropertyValue, new[] { sourcePropertyValue });
        }
    }

    protected virtual bool TryBindUIObjectPropertyCollectionRecursively(PropertyInfo sourcePropertyInfo,
                                                                        object sourcePropertyValue,
                                                                        PropertyInfo destinationPropertyInfo,
                                                                        object destinationPropertyValue)
    {
        var sourcePropertyType = sourcePropertyValue.GetType();
        var destinationPropertyType = destinationPropertyValue.GetType();
        var isUIObjectPropertyCollection =
            sourcePropertyType.IsGenericType &&
            sourcePropertyType.GetGenericTypeDefinition() == typeof(UIPropertyCollection<>) &&
            sourcePropertyType.GetGenericArguments()[0].IsAssignableTo(typeof(UIObject));

        if (!isUIObjectPropertyCollection) return false;

        var itemType = sourcePropertyType.GetGenericArguments()[0];

        // Get BindSourceToDestination(T source, Func<T, T> cloneFunc)
        var bindMethod = destinationPropertyType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(method =>
                method.Name == nameof(UIProperty<object>.BindSourceToDestination) &&
                method.GetParameters().Length == 2 &&
                method.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == sourcePropertyType.GetGenericTypeDefinition() &&
                method.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        if (bindMethod is null) throw new ArgumentException($"Cannot find BindSourceToDestination({sourcePropertyType}, {sourcePropertyType} => {destinationPropertyType})");

        // Create Func<T, T> where T : UIObject → item => Clone(item)
        var parameter = Expression.Parameter(itemType, "item");
        var cloneMethod = GetType().GetMethod(nameof(Clone), BindingFlags.Public | BindingFlags.Instance);
        if (cloneMethod is null) throw new InvalidOperationException($"Cannot find {nameof(Clone)} method");
        var body = cloneMethod.MakeGenericMethod(itemType);

        var call = Expression.Call(
            Expression.Constant(this),
            body,
            parameter);
        var lambda = Expression.Lambda(
            typeof(Func<,>).MakeGenericType(itemType, itemType),
            call, parameter).Compile();

        var closedBindMethod = bindMethod.MakeGenericMethod(itemType);
        closedBindMethod.Invoke(destinationPropertyValue, new[] { sourcePropertyValue, lambda });

        return true;
    }
}
