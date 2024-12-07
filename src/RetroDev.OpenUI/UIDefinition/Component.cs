using System.Diagnostics;

namespace RetroDev.OpenUI.UIDefinition;

[DebuggerDisplay("{Name}")]
public class Component(string name, List<Attribute> attributes, List<Component> components)
{
    public string Name { get; set; } = name;
    public List<Attribute> Attributes { get; set; } = attributes;
    public List<Component> Components { get; set; } = components;
}
