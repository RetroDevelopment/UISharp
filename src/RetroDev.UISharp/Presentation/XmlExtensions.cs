using System.Xml.Linq;

namespace RetroDev.UISharp.Presentation;

internal static class XmlExtensions
{
    public static string? GetAttributeIgnoreCase(this XElement @this, string name) =>
        @this.Attributes()
             .Where(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))
             .Select(a => a.Value)
             .FirstOrDefault();
}
