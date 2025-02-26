using System.ComponentModel;
using RetroDev.UISharp.Components;
using RetroDev.UISharp.UIDefinition.Ast;
using RetroDev.UISharp.UIDefinition.Exceptions;
using Attribute = RetroDev.UISharp.UIDefinition.Ast.Attribute;
using Component = RetroDev.UISharp.UIDefinition.Ast.Component;

namespace RetroDev.UISharp.UIDefinition;

public class Validator
{
    private TypeMapper _typeMapper;

    public Validator(TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Validate(Component component)
    {
        var exceptions = new List<UIDefinitionValidationException>();

        var componentType = _typeMapper.GetUIComponent(component.Name);
        if (componentType == null)
        {
            exceptions.Add(new UIDefinitionValidationException($"Cannot find UI component {component.Name}", component));
        }
        else
        {
            exceptions.AddRange(ValidateConstructor(component, componentType));
            exceptions.AddRange(ValidateAttributes(component, componentType));
        }

        foreach (var childComponent in component.Components)
        {
            try
            {
                Validate(childComponent);
            }
            catch (UIDefinitionValidationCompoundException e)
            {
                exceptions.AddRange(e.Exceptions);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new UIDefinitionValidationCompoundException(exceptions);
        }
    }

    private List<UIDefinitionValidationException> ValidateConstructor(Component component, Type componentType)
    {
        try
        {
            _typeMapper.GetConstructor(componentType);
        }
        catch (Exception e)
        {
            return [new UIDefinitionValidationException(e.Message, component, e)];
        }

        return [];
    }

    private List<UIDefinitionValidationException> ValidateAttributes(Component component, Type componentType)
    {
        var exceptions = new List<UIDefinitionValidationException>();

        foreach (var attribute in component.Attributes)
        {
            if (_typeMapper.GetUIProperty(componentType, attribute.Name) == null)
            {
                exceptions.Add(new UIDefinitionValidationException($"Property {attribute.Name} does not exist in component {component.Name}", attribute));
            }
        }

        return exceptions;
    }
}
