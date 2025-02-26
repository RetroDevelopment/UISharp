using RetroDev.UISharp.UIDefinition.Ast;
using Attribute = RetroDev.UISharp.UIDefinition.Ast.Attribute;

namespace RetroDev.UISharp.UIDefinition;

/// <summary>
/// Generates code from the UI definition AST.
/// </summary>
public class CodeGenerator
{
    /// <summary>
    /// Wheter xml spacing should be done using tabs or spaces.
    /// </summary>
    public bool UseTabs { get; set; } = false;

    /// <summary>
    /// Generates UI definition code for the given <paramref name="component"/>.
    /// </summary>
    /// <param name="component">The component for which to generate the UI definition code.</param>
    /// <returns>The xml UI definition code.</returns>
    public string Generate(Component component)
    {
        var xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
        xml += InternalGenerate(component, string.Empty);
        return xml;
    }

    private string InternalGenerate(Component component, string indentation)
    {
        var indentationCharacters = UseTabs ? "\t" : "    ";
        var xmlAttributeList = component.Attributes.Select(InternalGenerate);
        var xmlAttributesString = string.Join(' ', xmlAttributeList);

        if (component.Components.Count != 0)
        {
            var space = xmlAttributeList.Any() ? " " : string.Empty;
            var xml = $"{indentation}<{component.Name}{space}{xmlAttributesString}>\n";
            var xmlChildrenList = component.Components.Select(c => InternalGenerate(c, $"{indentation}{indentationCharacters}"));
            xml += string.Join(string.Empty, xmlChildrenList);
            xml += $"{indentation}</{component.Name}>\n";
            return xml;
        }
        else
        {
            return $"{indentation}<{component.Name} {xmlAttributesString} />\n";
        }
    }

    private string InternalGenerate(Attribute attribute) =>
        $"{attribute.Name.ToLower()}=\"{attribute.Value}\"";
}
