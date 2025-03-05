using System.Xml.Linq;
using System.Xml;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Themes.Exceptions;
using RetroDev.UISharp.Presentation.Resources;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.Components.Base;

namespace RetroDev.UISharp.Presentation.Themes;

/// <summary>
/// Manages themes.
/// This class allows dynamically changing themes in the whole application by updating the respective
/// color bindings via <see cref="BindableProperty{TValue}"/>.
/// </summary>
/// <param name="application">The application owning <see langword="this" /> <see cref="ThemeManager"/>.</param>
/// <param name="themeResources">The object that loads theme xml files.</param>
public class ThemeManager(Application application, ITextResources themeResources)
{
    private readonly Application _application = application;
    private readonly Dictionary<string, BindableProperty<Color>> _colorProperties = [];
    private Dictionary<string, Color> _currentTheme = [];

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
            var document = XDocument.Parse(xml);
            if (document?.Root == null) throw new ThemeParseException("Failed to parse ui definition xml");
            var rootName = document.Root.Name.LocalName;
            if (!rootName.Equals("theme", StringComparison.OrdinalIgnoreCase)) throw new ThemeParseException($"Root theme tag xml must be theme, found {rootName} instead.");
            _currentTheme = document.Root.Elements().Select(Parse).ToDictionary();
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

    private KeyValuePair<string, Color> Parse(XElement element)
    {
        var attributes = element.Attributes().ToList();
        var key = element.Name.LocalName;
        var hexColor = attributes.Where(a => a.Name.LocalName.Equals("color", StringComparison.OrdinalIgnoreCase))
                                 .Select(a => a.Value)
                                 .FirstOrDefault();

        if (hexColor == null) throw new ThemeParseException($"Cannot find color attribute in color tag {element}");
        return new KeyValuePair<string, Color>(key, new Color(hexColor));
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
