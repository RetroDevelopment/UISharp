using RetroDev.OpenUI.Components.Base;
using RetroDev.OpenUI.Presentation.Properties;
using System.Reflection;

namespace RetroDev.OpenUI.UIDefinition;

/// <summary>
/// Finds the relevant UI types defined in the OpenUI as well as the types defined in the project using OpenUI.
/// </summary>
public class TypeMapper
{
    private List<Type> _allTypes;
    private Dictionary<Type, List<PropertyInfo>> _uiComponents;

    /// <summary>
    /// Creates a new UIDefinition type mapper.
    /// It scans all the UI-related types in all the loaded assemblies.
    /// </summary>
    public TypeMapper()
    {
        _allTypes = LoadAllTypesFromAllAssemblies();
        _uiComponents = LoadUIComponents();
    }

    /// <summary>
    /// Updates the list of UI types found in projects. This is useful when adding or removing assemblies to the project at runtime.
    /// </summary>
    public void ReloadTypes()
    {
        _allTypes = LoadAllTypesFromAllAssemblies();
        _uiComponents = LoadUIComponents();
    }

    /// <summary>
    /// Checks whether a type derived from <see cref="UIComponent"/> with the given <paramref name="name"/> exists in the loaded assemblies.
    /// </summary>
    /// <param name="name">The name of the type to find. It is case-insensitive and it can be either <see cref="Type.Name"/> or <see cref="Type.FullName"/>.</param>
    /// <returns><see langword="true" /> if the type exists, otherwise <see langword="false" />.</returns>
    public bool UIComponentExists(string name) =>
        GetUIComponent(name) != null;

    /// <summary>
    /// Gets the <see cref="Type"/> derived from <see cref="UIComponent"/> with the given <paramref name="name"/> if such component exists in the loaded assembiles.
    /// </summary>
    /// <param name="name">The name of the type to find. It is case-insensitive and it can be either <see cref="Type.Name"/> or <see cref="Type.FullName"/>.</param>
    /// <returns>The found type if any, otherwise <see langword="null" /></returns>
    public Type? GetUIComponent(string name)
    {
        var lowerCaseName = name.ToLower();
        bool NameMatchesUIComponentName(Type uiComponentType)
        {
            var nameMatch = uiComponentType.Name.ToLower().Equals(lowerCaseName, StringComparison.CurrentCultureIgnoreCase);
            var fullNameMatch = (uiComponentType.FullName?.ToLower() ?? string.Empty).Equals(lowerCaseName, StringComparison.CurrentCultureIgnoreCase);
            return nameMatch || fullNameMatch;
        }

        return _uiComponents.Keys.Where(NameMatchesUIComponentName).FirstOrDefault();
    }

    /// <summary>
    /// Gets the constructor needed to create a new instance of a <see cref="UIComponent"/> of the given <paramref name="componentType"/>.
    /// </summary>
    /// <param name="componentType">The type of the constructor to search for.</param>
    /// <returns>The found constructor.</returns>
    /// <exception cref="ArgumentException">If a suitable constructor cannot be found.</exception>
    public ConstructorInfo GetConstructor(Type componentType)
    {
        var constructor = componentType.GetConstructors()
                                       .FirstOrDefault(ctor =>
                                       {
                                           var parameters = ctor.GetParameters();
                                           return parameters.Length > 0 && parameters[0].ParameterType == typeof(Application);
                                       });

        if (constructor == null)
        {
            throw new ArgumentException($"Cannot find a suitable constructor for {componentType.FullName}: a constructor in the form ctor(Application, ...) must be defined.");
        }

        var parameters = constructor.GetParameters();
        if (parameters.TakeLast(parameters.Count() - 1).Any(p => !p.IsOptional && !p.ParameterType.IsAssignableTo(typeof(UIComponent))))
        {
            throw new ArgumentException($"Constructor for {componentType.FullName} must have all optional parameters except for the first parameter which must mandatory and of type Application, and for possibly parameters derived from UIComponent");
        }

        return constructor;
    }

    /// <summary>
    /// Finds an instance of <see cref="UIProperty{TParent, TValue}"/> with the given <paramref name="name"/> in the given <see cref="Type"/>.
    /// </summary>
    /// <param name="component">The component type. It must be a type assignable to <see cref="UIComponent"/>.</param>
    /// <param name="name">The name of the property to look for.</param>
    /// <returns>The property reflection information.</returns>
    public PropertyInfo? GetUIProperty(Type component, string name)
    {
        if (!component.IsAssignableTo(typeof(UIComponent))) throw new ArgumentException($"Component type {component.FullName} is not assignable to {nameof(UIComponent)}");
        if (!_uiComponents.ContainsKey(component)) throw new ArgumentException($"Cannot find UIComponent with name {name}");
        return _uiComponents[component].Where(p => p.Name.ToLower() == name.ToLower()).FirstOrDefault();
    }

    /// <summary>
    /// Gets the list of all types derived from <see cref="UIComponent"/>.
    /// </summary>
    /// <returns>The list of all types derived from <see cref="UIComponent"/>.</returns>
    public IEnumerable<Type> GetComponentTypes() =>
        _uiComponents.Keys;

    /// <summary>
    /// Gets the list of all proporties (derived from <see cref="UIProperty{TParent, TValue}") in the given component <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The UI component type for which to get the properties. It must be derived from <see cref="UIComponent"/>.</param>
    /// <returns>The list of all found property reflection infromation.</returns>
    public IEnumerable<PropertyInfo> GetProperties(Type type) =>
        _uiComponents[type];

    /// <summary>
    /// Gets all the types in all loaded assemblies.
    /// </summary>
    /// <returns>The list of all loaded types.</returns>
    public IEnumerable<Type> GetAllTypes() => _allTypes;

    private Dictionary<Type, List<PropertyInfo>> LoadUIComponents()
    {
        var result = new Dictionary<Type, List<PropertyInfo>>();
        var uiComponentTypes = GetDerivedTypes<UIComponent>(_allTypes);

        foreach (var type in uiComponentTypes)
        {
            var uiProperties = GetUIProperties(type);
            if (uiProperties.Count != 0)
            {
                result[type] = uiProperties;
            }
        }

        return result;
    }

    private List<Type> LoadAllTypesFromAllAssemblies()
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
            }).ToList();
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
