using System.Reflection;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Exceptions;
using Component = RetroDev.OpenUI.UIDefinition.Ast.Component;

namespace RetroDev.OpenUI.UIDefinition;

/// <summary>
/// Creates and instance of a <see cref="UIComponent"/> from an xml UIDefinition.
/// </summary>
/// <param name="application">The application that owns <see langword="this" /> object.</param>
/// <param name="typeMapper">The object that find the relevant UI types defined in the OpenUI as well as the types defined in the project using OpenUI.</param>
/// <param name="binder">
/// The binder to define the EAML language, which binds UIDefinition attributes with actual values.
/// If this parameter is not specified, a <see cref="EAMLBinder"/> isntance will be created, using the default EAML implementation.
/// </param>
public class InstanceCreator(Application application, TypeMapper typeMapper, IEAMLBinder? binder = null)
{
    private readonly Application _application = application;
    private readonly TypeMapper _typeMapper = typeMapper;
    private readonly IEAMLBinder _binder = binder ?? new EAMLBinder(typeMapper);

    /// <summary>
    /// Creates a <see cref="UIComponent"/> instance as defined in the given <paramref name="component"/> AST.
    /// </summary>
    /// <param name="component">The UIDefinition AST component to use to create the <see cref="UIComponent"/> instance.</param>
    /// <returns></returns>
    /// <exception cref="UIDefinitionValidationException">If the given <paramref name="component"/> is invalid. Call <see cref="Validator.Validate(Component)" beforehand to avoid this exception./></exception>
    public UIComponent CreateUIComponent(Component component)
    {
        var componentType = _typeMapper.GetUIComponent(component.Name) ?? throw new UIDefinitionValidationException($"Component {component.Name} does not map to a known type.", component);
        var constructor = _typeMapper.GetConstructor(componentType);
        var childComponentInstances = component.Components.Select(CreateUIComponent).ToList();
        var arguments = PrepareConstructorArguments(constructor.GetParameters(), childComponentInstances.SelectMany(c => c.GetComponentTreeNodesDepthFirstSearch()).Union(childComponentInstances), component);
        var componentInstance = (UIComponent)constructor.Invoke(arguments) ?? throw new UIDefinitionValidationException($"Failed to invoke constructor for {component.Name}", component);
        InitializeUIComponent(component, componentInstance, childComponentInstances);
        if (componentInstance is Window window) window.OnInitialized();
        return componentInstance;
    }

    private object?[] PrepareConstructorArguments(ParameterInfo[] parameters, IEnumerable<UIComponent> components, Component component)
    {
        var arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (i == 0 && parameter.ParameterType == typeof(Application))
            {
                arguments[i] = _application;
            }
            else if (parameter.IsOptional)
            {
                arguments[i] = parameter.DefaultValue ?? GetDefaultValue(parameter.ParameterType);
            }
            else if (parameter.ParameterType.IsAssignableTo(typeof(UIComponent)))
            {
                var matchArgument = components.Where(c => c.GetType() == parameter.ParameterType && c.ID.Value.ToLower() == parameter.Name?.ToLower())
                                              .FirstOrDefault();
                if (matchArgument != null)
                {
                    arguments[i] = matchArgument;
                }
                else
                {
                    throw new UIDefinitionValidationException($"Cannot find suitable UIComponent for constructor parameter {parameter.Name}: make sure parameter name and UIComponent ID match", component);
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot resolve value for non-optional parameter '{parameter.Name}' of type '{parameter.ParameterType}'");
            }
        }

        return arguments;
    }

    private object? GetDefaultValue(Type type)
    {
        // Return a default value for the type (e.g., null for reference types, default(T) for value types)
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    private void InitializeUIComponent(Component component, UIComponent instance, IEnumerable<UIComponent> childrenInstances)
    {
        foreach (var attribute in component.Attributes)
        {
            var property = _typeMapper.GetUIProperty(instance.GetType(), attribute.Name) ?? throw new ArgumentException($"Cannot find UI property {attribute.Name}: make sure the property is of type UIProperty");
            _binder.SetUIProperty(property, attribute, instance);
        }

        var i = 0;

        foreach (var childInstance in childrenInstances)
        {
            var childComponent = component.Components[i];
            if (instance is Components.IContainer multipleContainer)
            {
                multipleContainer.AddComponent(childInstance);
            }
            else if (instance is ISingleContainer singleContainer)
            {
                if (component.Components.Count == 1)
                {
                    singleContainer.SetComponent(childInstance);
                }
                else
                {
                    throw new UIDefinitionValidationException($"Only one child allowed in {singleContainer.GetType().Name}", component);
                }
            }
            else
            {
                throw new UIDefinitionValidationException($"{instance.GetType().Name} is not a container so it cannot contain children (make sure the type implements IContainer or ISingleContainer)", component);
            }

            i++;
        }
    }
}
