using System.Reflection;
using RetroDev.OpenUI.Components;

namespace RetroDev.OpenUI.UIDefinition;

// No thread safe?
public class UIDefinitionManager
{
    private readonly Dictionary<string, Component> _definitions = [];

    public Application Application { get; }
    public TypeMapper TypeMapper { get; }
    public UIDefinitionParser Parser { get; }
    public UIDefinitionInstanceCreator InstanceCreator { get; }

    public UIDefinitionCodeGenerator CodeGenerator { get; }

    public UIDefinitionManager(Application application)
    {
        Application = application;
        TypeMapper = new TypeMapper();
        Parser = new UIDefinitionParser();
        InstanceCreator = new UIDefinitionInstanceCreator(Application, TypeMapper);
        CodeGenerator = new UIDefinitionCodeGenerator();
    }

    public void AddDefinition(string id, string xml)
    {
        if (_definitions.ContainsKey(id)) throw new ArgumentException($"UI Definition with id {id} already exists");

        var parser = new UIDefinitionParser();
        var validator = new UIDefinitionValidator(TypeMapper);
        var componentDefinition = parser.Parse(xml);
        validator.Validate(componentDefinition);
        _definitions.Add(id, componentDefinition);
    }

    public Component GetDefinition(string id)
    {
        if (!_definitions.ContainsKey(id)) throw new ArgumentException($"UI Definition with id {id} does not exist");
        return _definitions[id];
    }

    public UIComponent CreateUIComponent(Component component) =>
        InstanceCreator.CreateUIComponent(component);

    public UIComponent CreateUIComponent(string xml)
    {
        var parser = new UIDefinitionParser();
        var validator = new UIDefinitionValidator(TypeMapper);
        var componentDefinition = parser.Parse(xml);
        validator.Validate(componentDefinition);
        return InstanceCreator.CreateUIComponent(componentDefinition);
    }
}
