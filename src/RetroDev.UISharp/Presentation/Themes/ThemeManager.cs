using System.Xml.Linq;
using System.Xml;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Themes.Exceptions;
using RetroDev.UISharp.Presentation.Resources;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Presentation.Themes;

using ColorDictionary = Dictionary<string, Color>;

/// <summary>
/// Manages themes.
/// This class allows dynamically changing themes in the whole application by updating the respective
/// color bindings via <see cref="BindableProperty{TValue}"/>.
/// </summary>
/// <param name="application">The application owning <see langword="this" /> <see cref="ThemeManager"/>.</param>
/// <param name="themeResources">The object that loads theme xml files.</param>
/// <remarks>
/// The theme xml is in the form: <br />
/// &lt;theme name="name" include="theme-name"&gt; <br />
/// &lt;colorName1 color="#RRGGBBAA" /&gt; <br />
/// &lt;colorName2 link="colorName1" /&gt; <br />
/// ... <br />
/// &lt;/theme&gt; <br />
/// The theme will be a list of mapping from <c>colorName1</c> to the color in the RRGGBBAA hex format,
/// or mappings from <c>colorName2</c> to another mapping.
/// The <c>include</c> attribute is optional and it indicates that the theme also includes the theme entries from
/// the specified <c>theme-name</c> resource (loaded using the given <paramref name="themeResources"/>).
/// The theme name is just a remark, it is not processed by any mean.
/// Note that it is NOT possible to link a color to another color with the <c>link</c> attribute, but the linked color (the ID inside the link attribute)
/// must be a color tag with the <c>color</c> attribute.
/// It is possible to override a color attribute and it is done in order: included themes are processed first.
/// </remarks>
public class ThemeManager(Application application, ITextResources themeResources)
{
    private const string Theme = "theme";
    private const string IncludeAttribute = "include";
    private const string ColorAttribute = "color";
    private const string LinkAttribute = "link";

    private readonly Application _application = application;
    private readonly Dictionary<string, BindableProperty<Color>> _colorProperties = [];
    private ColorDictionary _currentTheme = [];

    /// <summary>
    /// The object that loads theme xml files.
    /// </summary>
    public ITextResources ThemeResources { get; set; } = themeResources;

    /// <summary>
    /// Parses the given <paramref name="xml"/> theme and set it as current theme.
    /// </summary>
    /// <param name="xml">The theme xml.</param>
    /// <exception cref="ThemeParseException">If parsing fails.</exception>
    public void ParseTheme(string xml)
    {
        try
        {
            _application.Logger.LogDebug($"Theme xml: {xml}");
            Dictionary<string, string> links = [];
            _currentTheme = InternalParseTheme(xml, links);
            foreach (var link in links)
            {
                ResolveLink(link.Key, link.Value, _currentTheme, links);
            }

            UpdateThemeProperties();
        }
        catch (XmlException e)
        {
            throw new ThemeParseException($"Failed to parse ui definition xml: {e.Message}", e);
        }
    }

    /// <summary>
    /// Loads the theme identified by the given <paramref name="resourceName"/> using the <see cref="ThemeResources"/>
    /// manager and sets it as current theme.
    /// </summary>
    /// <param name="resourceName">The name of the resource theme to load.</param>
    public void LoadTheme(string resourceName)
    {
        _application.Logger.LogInfo($"Theme: {resourceName}");
        var xml = ThemeResources[resourceName];
        ParseTheme(xml);
    }

    /// <summary>
    /// Gets the color <see cref="BindableProperty{TValue}"/> bound to the given <paramref name="id"/>.
    /// There is one <see cref="BindableProperty{TValue}" /> for each theme id, which is the same
    /// even if the theme changes. This means that it is possible to change theme and instantly update
    /// the whole UI via bindings.
    /// </summary>
    /// <param name="id">The color identifier. In the xml this is a color tag name.</param>
    /// <returns>The <see cref="BindableProperty{TValue}"/> for this color.</returns>
    /// <exception cref="ArgumentException">If a color with the given <paramref name="id"/> does not exist in the current theme.</exception>
    /// <remarks>
    /// DO NOT assign the returned property to a UI component directly as this property is used to define a theme binding.
    /// If you want to create a new color property for a UI component bound to a theme, use <see cref="ThemeExtensions.CreateNewColorPropertyFor{TComponent}(TComponent, string)"/> instead.
    /// </remarks>
    public BindableProperty<Color> GetColorProperty(string id)
    {
        if (_currentTheme.Count == 0) throw new ArgumentException($"Color id {id} does not exist in current theme: current theme is empty.");
        if (!_currentTheme.ContainsKey(id)) throw new ArgumentException($"Color id {id} does not exist in current theme");
        return _colorProperties[id];
    }

    private ColorDictionary InternalParseTheme(string xml, Dictionary<string, string> links)
    {
        try
        {
            var document = XDocument.Parse(xml);
            if (document?.Root == null) throw new ThemeParseException("Failed to parse color theme xml");
            var rootName = document.Root.Name.LocalName;
            if (!rootName.Equals(Theme, StringComparison.OrdinalIgnoreCase)) throw new ThemeParseException($"Root theme tag xml must be theme, found {rootName} instead.");
            var include = document.Root.GetAttributeIgnoreCase(IncludeAttribute);
            ColorDictionary themeDictionary = [];

            if (include != null)
            {
                var includedXml = ThemeResources[include];
                themeDictionary = InternalParseTheme(includedXml, links);
            }

            themeDictionary = themeDictionary.Union(document.Root.Elements().Select(e => ParseColors(e, links))).ToDictionary();

            return themeDictionary;
        }
        catch (XmlException e)
        {
            throw new ThemeParseException($"Failed to parse ui definition xml: {e.Message}", e);
        }
    }

    private KeyValuePair<string, Color> ParseColors(XElement element, Dictionary<string, string> links)
    {
        var key = element.Name.LocalName;
        var hexColor = element.GetAttributeIgnoreCase(ColorAttribute);
        var link = element.GetAttributeIgnoreCase(LinkAttribute);

        if (link != null)
        {
            links.Add(key, link);
            return new KeyValuePair<string, Color>(key, Color.Transparent);
        }
        else if (hexColor != null)
        {
            links.Remove(key);
            return new KeyValuePair<string, Color>(key, new Color(hexColor));
        }
        else
        {
            throw new ThemeParseException($"Cannot find color attribute in color tag {element}");
        }
    }

    private void ResolveLink(string key, string linkKey, ColorDictionary dictionary, Dictionary<string, string> links)
    {
        if (links.ContainsKey(linkKey)) _application.Logger.LogWarning($"Failed to link color {key} to color {linkKey}: cannot link a color to another color with the link attribute");
        else if (dictionary.ContainsKey(linkKey)) dictionary[key] = dictionary[linkKey];
        else _application.Logger.LogWarning($"Failed to link color {key} to color {linkKey}: {linkKey} does not exist");
    }

    private void UpdateThemeProperties()
    {
        foreach (var idColorBinding in _currentTheme)
        {
            if (!_colorProperties.ContainsKey(idColorBinding.Key))
            {
                _colorProperties.Add(idColorBinding.Key, new BindableProperty<Color>(idColorBinding.Value, _application, BindingType.SourceToDestination));
            }
            else
            {
                _colorProperties[idColorBinding.Key].Value = idColorBinding.Value;
            }
        }
    }
}
