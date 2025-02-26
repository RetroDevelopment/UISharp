using System.Diagnostics;

namespace RetroDev.UISharp.UIDefinition.Ast;

[DebuggerDisplay("{Name}={Value}")]
public class Attribute(string name, string value) : Node
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
}
