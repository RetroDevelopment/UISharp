using System.ComponentModel;
using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.UIDefinition;

// TODO: add more to validation logic + return a list of errors instead of a single error, so that we know more (also add it as list of errors with error line and column).

public class UIDefinitionValidator
{
    private TypeMapper _typeMapper;

    public UIDefinitionValidator(TypeMapper typeMapper)
    {
        _typeMapper = typeMapper;
    }

    public void Validate(Component component)
    {
        var componentType = _typeMapper.GetUIComponent(component.Name);
        if (componentType == null) throw new UIDefinitionValidationException($"Cannot find UI component {component.Name}");

        foreach (var attribute in component.Attributes)
        {
            if (!_typeMapper.UiPropertyExists(componentType)) throw new UIDefinitionValidationException($"Property {attribute.Name} does not exist in component {component.Name}");
        }

        component.Components.ForEach(Validate);
    }
}
