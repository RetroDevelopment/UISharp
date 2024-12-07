using System.Xml;
using System.Xml.Linq;
using RetroDev.OpenUI.Exceptions;

namespace RetroDev.OpenUI.UIDefinition;

public class UIDefinitionParser
{
    public Component Parse(string xml)
    {
        try
        {
            var document = XDocument.Parse(xml);
            if (document?.Root == null) throw new ArgumentException("Failed to parse ui definition xml");
            return Parse(document.Root);
        }
        catch (XmlException e)
        {
            // If UIDefinition language is no longer an xml in the future, we will still throw ArgumentException with
            // a different xml exception.
            throw new UIDefinitionValidationException($"Failed to parse ui definition xml: {e.Message}", e);
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
