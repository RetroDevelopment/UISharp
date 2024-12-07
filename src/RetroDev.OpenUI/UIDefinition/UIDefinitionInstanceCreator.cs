using System.ComponentModel;
using System.Reflection;
using RetroDev.OpenUI.Components;
using RetroDev.OpenUI.Components.AutoSize;
using RetroDev.OpenUI.Core.Coordinates;
using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.UIDefinition;

public class UIDefinitionInstanceCreator(Application application, TypeMapper typeMapper)
{
    private readonly Application _application = application;
    private readonly TypeMapper _typeMapper = typeMapper;

    public UIComponent CreateUIComponent(Component component)
    {
        var componentType = _typeMapper.GetUIComponent(component.Name) ?? throw new UIDefinitionValidationException($"Component {component.Name} does not map to a known type.");
        var constructor = componentType.GetConstructors()
            .FirstOrDefault(ctor =>
            {
                var parameters = ctor.GetParameters();
                return parameters.Length > 0 && parameters[0].ParameterType == typeof(Application);
            }) ?? throw new InvalidOperationException($"Component {component.Name} does not have a suitable constructor starting with (Application)");

        var arguments = PrepareConstructorArguments(constructor.GetParameters());
        var componentInstance = (UIComponent)constructor.Invoke(arguments) ?? throw new UIDefinitionValidationException($"Failed to invoke constructor for {component.Name}");
        InitializeUIComponent(component, componentInstance);
        if (componentInstance is Window window) window.OnInitialized();
        return componentInstance;
    }

    private object?[] PrepareConstructorArguments(ParameterInfo[] parameters)
    {
        var arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            // Provide the application for the first parameter
            if (i == 0 && parameter.ParameterType == typeof(Application))
            {
                arguments[i] = _application;
            }
            else if (parameter.IsOptional)
            {
                // Use the default value for optional parameters
                arguments[i] = parameter.DefaultValue ?? GetDefaultValue(parameter.ParameterType);
            }
            else
            {
                // For non-optional parameters, throw an exception if no value is provided
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

    private void InitializeUIComponent(Component component, UIComponent instance)
    {
        foreach (var attribute in component.Attributes)
        {
            var property = _typeMapper.GetUIProperty(instance.GetType(), attribute.Name) ?? throw new ArgumentException($"Cannot find UI property {attribute.Name}: make sure the property is of type UIProperty");
            InitializeAttribute(attribute, property, instance);
        }

        foreach (var child in component.Components)
        {
            var childInstance = CreateUIComponent(child);
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
                    throw new UIDefinitionValidationException($"Only one child allowed in {singleContainer.GetType().Name}");
                }
            }
            else
            {
                throw new UIDefinitionValidationException($"{instance.GetType().Name} is not a container so it cannot contain children (make sure the type implements IContainer or ISingleContainer)");
            }
        }
    }

    private void InitializeAttribute(Attribute attribute, PropertyInfo propertyInfo, UIComponent componentInstance)
    {
        var propertyValueType = propertyInfo.PropertyType;
        var propertyValue = propertyInfo.GetValue(componentInstance) ?? throw new NullReferenceException("Property value cannot be null");
        var propertyValueInfo = propertyValue.GetType().GetProperty("Value") ?? throw new InvalidOperationException("Property value Value inner property not found");
        var propertyType = propertyValueInfo.PropertyType;
        propertyValueInfo.SetValue(propertyValue, DynamicCast(attribute.Value, propertyType)); // Converts xml attribute value into the relevant type T assuming propertyInfo.PropertyType is UIProperty<..., T>
    }

    // TODO: create a Converter class that can be extended by users.
    private object DynamicCast(string attribute, Type type)
    {
        // Handle PixelUnit specifically
        if (type == typeof(PixelUnit))
        {
            if (attribute.Equals("auto", StringComparison.OrdinalIgnoreCase))
            {
                return PixelUnit.Auto;
            }

            // Assume a method exists to convert string to float
            if (float.TryParse(attribute, out float floatValue))
            {
                return new PixelUnit(floatValue);
            }

            throw new UIDefinitionValidationException($"Invalid value for {nameof(PixelUnit)}: {attribute}");
        }

        // Handle IAutoSizeStrategy specifically
        if (type == typeof(IAutoSizeStrategy))
        {
            if (attribute.Equals("match.parent", StringComparison.OrdinalIgnoreCase))
            {
                return AutoSizeStrategy.MatchParent;
            }
            else if (attribute.Equals("wrap.left", StringComparison.OrdinalIgnoreCase) ||
                     attribute.Equals("wrap.top", StringComparison.OrdinalIgnoreCase))
            {
                return AutoSizeStrategy.WrapComponentLeftTop;
            }
            else if (attribute.Equals("wrap.center", StringComparison.OrdinalIgnoreCase))
            {
                return AutoSizeStrategy.WrapComponentRightBottom;
            }
            else if (attribute.Equals("wrap.bottom", StringComparison.OrdinalIgnoreCase) ||
                     attribute.Equals("wrap.right", StringComparison.OrdinalIgnoreCase))
            {
                return AutoSizeStrategy.WrapComponentRightBottom;
            }
            else
            {
                throw new UIDefinitionValidationException($"Invalid value for {nameof(IAutoSizeStrategy)}: {attribute}");
            }
        }

        // Handle primitive types and other basic conversions
        try
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(type);
            if (converter != null && converter.IsValid(attribute))
            {
                return converter.ConvertFromInvariantString(attribute) ?? throw new ArgumentException("Convertion failed unexpectectly");
            }
        }
        catch
        {
            throw new UIDefinitionValidationException($"Cannot convert value '{attribute}' to type {type.Name}: convertion exception");
        }

        throw new UIDefinitionValidationException($"Cannot convert value '{attribute}' to type {type.Name}");
    }
}
