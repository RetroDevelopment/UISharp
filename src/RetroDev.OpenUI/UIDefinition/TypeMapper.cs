using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Properties;
using System.Reflection;

namespace RetroDev.OpenUI.UIDefinition;

public class TypeMapper
{
    private Dictionary<Type, List<PropertyInfo>> _uiComponents;

    public TypeMapper()
    {
        _uiComponents = LoadUIComponents();
    }

    public void ReloadTypes() =>
        _uiComponents = LoadUIComponents();

    public bool UIComponentExists(string name) =>
        GetUIComponent(name) != null;

    public Type? GetUIComponent(string name) =>
        _uiComponents.Keys.Where(c => c.Name.ToLower() == name.ToLower() ||
                                      c.FullName?.ToLower() == name.ToLower()).FirstOrDefault();

    public PropertyInfo? GetUIProperty(Type component, string name) =>
        _uiComponents[component].Where(p => p.Name.ToLower() == name.ToLower()).FirstOrDefault();

    public bool UiPropertyExists(Type component) =>
        GetUIProperties(component) != null;

    public IEnumerable<Type> GetComponentTypes() =>
        _uiComponents.Keys;

    public IEnumerable<PropertyInfo> GetProperties(Type type) =>
        _uiComponents[type];

    private Dictionary<Type, List<PropertyInfo>> LoadUIComponents()
    {
        var result = new Dictionary<Type, List<PropertyInfo>>();
        var allTypes = GetAllTypes();
        var uiComponentTypes = GetDerivedTypes<UIComponent>(allTypes);

        foreach (var type in uiComponentTypes)
        {
            // Get all properties of the type where the property's type is UIProperty
            var uiProperties = GetUIProperties(type);

            if (uiProperties.Any())
            {
                result[type] = uiProperties;
            }
        }

        return result;
    }

    private IEnumerable<Type> GetAllTypes()
    {
        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null).Cast<Type>();
                }
            });
    }


    private IEnumerable<Type> GetDerivedTypes<TBase>(IEnumerable<Type> types)
    {
        return types.Where(t => t.IsClass && !t.IsAbstract && typeof(TBase).IsAssignableFrom(t));
    }

    private List<PropertyInfo> GetUIProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                   .Where(prop => prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(UIProperty<,>))
                   .ToList();
    }
}
