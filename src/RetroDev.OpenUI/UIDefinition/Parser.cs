using System.Xml;
using System.Xml.Linq;
using RetroDev.OpenUI.UIDefinition.Ast;
using RetroDev.OpenUI.UIDefinition.Exceptions;
using Attribute = RetroDev.OpenUI.UIDefinition.Ast.Attribute;

namespace RetroDev.OpenUI.UIDefinition;

/// <summary>
/// The UIDefinition language parser.
/// </summary>
public class Parser
{
    /// <summary>
    /// Parse the given UIDefinition <paramref name="xml"/> text.
    /// </summary>
    /// <param name="xml">The UIDefinition xml text to parse.</param>
    /// <returns>The parsed UIDefinition AST.</returns>
    /// <exception cref="UIDefinitionParseException">If the XML is invalid.</exception>
    public Component Parse(string xml)
    {
        try
        {
            var document = XDocument.Parse(xml);
            if (document?.Root == null) throw new UIDefinitionParseException("Failed to parse ui definition xml");
            return Parse(document.Root);
        }
        catch (XmlException e)
        {
            // If UIDefinition language is no longer an xml in the future, we will still throw ArgumentException with
            // a different xml exception.
            throw new UIDefinitionParseException($"Failed to parse ui definition xml: {e.Message}", e);
        }
    }

    private Component Parse(XElement element)
    {
        var name = element.Name.LocalName.ToLower();
        var attributes = element.Attributes().Select(ParseAttribute).ToList();
        var children = element.Elements().Select(Parse).ToList();

        return new(name, attributes, children);
    }

    private Attribute ParseAttribute(XAttribute attribute)
    {
        return new Attribute(attribute.Name.LocalName.ToLower(), attribute.Value);
    }
}
