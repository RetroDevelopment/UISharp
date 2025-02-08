using RetroDev.OpenUI.Exceptions;
using System.Xml.Linq;
using System.Xml;
using System.Diagnostics.Contracts;
using RetroDev.OpenUI.Core.Graphics;

namespace RetroDev.OpenUI.UI.Themes;

/// <summary>
/// Parses the xml theme definition.
/// </summary>
public class ThemeParser
{
    private readonly Theme _theme;

    public ThemeParser(Theme theme)
    {
        _theme = theme;
    }

    /// <summary>
    /// Parses the given <paramref name="xml"/> theme.
    /// </summary>
    /// <param name="xml">The theme xml.</param>
    /// <exception cref="ThemeParseException">If parsing fails.</exception>
    public void Parse(string xml)
    {
        try
        {
            var document = XDocument.Parse(xml);
            if (document?.Root == null) throw new ThemeParseException("Failed to parse ui definition xml");
            var rootName = document.Root.Name.LocalName;
            if (!rootName.Equals("theme", StringComparison.OrdinalIgnoreCase)) throw new ThemeParseException($"Root theme tag xml must be theme, found {rootName} instead.");
            var dictionary = document.Root.Elements().Select(Parse).ToDictionary();
            _theme.SetColorDictionary(dictionary);
        }
        catch (XmlException e)
        {
            // If UIDefinition language is no longer an xml in the future, we will still throw ArgumentException with
            // a different xml exception.
            throw new ThemeParseException($"Failed to parse ui definition xml: {e.Message}", e);
        }
    }

    private KeyValuePair<string, Color> Parse(XElement element)
    {
        if (!element.Name.LocalName.Equals("color", StringComparison.OrdinalIgnoreCase)) throw new ThemeParseException($"Excepted color tag, found {element.Name.LocalName} instead.");

        var attributes = element.Attributes().ToList();
        var key = attributes.Where(a => a.Name.LocalName
                            .Equals("key", StringComparison.OrdinalIgnoreCase))
                            .Select(a => a.Value)
                            .FirstOrDefault();

        var hexColor = attributes.Where(a => a.Name.LocalName == "value")
                                 .Select(a => a.Value)
                                 .FirstOrDefault();

        if (key == null) throw new ThemeParseException($"Cannot find key attribute in color tag {element}");
        if (hexColor == null) throw new ThemeParseException($"Cannot find value attribute in color tag {element}");
        return new KeyValuePair<string, Color>(key, new Color(hexColor));
    }
}
