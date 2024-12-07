using System.Diagnostics;

namespace RetroDev.OpenUI.UIDefinition;

[DebuggerDisplay("{Name}={Value}")]
public class Attribute(string name, string value)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
}
