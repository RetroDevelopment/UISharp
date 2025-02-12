using System.Reflection;

namespace RetroDev.OpenUI.UI.Resources;

/// <summary>
/// The base class to load and manage resources embedded in the assemblies.
/// </summary>
/// <param name="resourceLocation">Location of the embedded resources search path inside the assemblies.</param>
public abstract class EmbeddedResourcesBase(string resourceLocation)
{
    private readonly string _resourceLocation = resourceLocation;
    private readonly Dictionary<string, byte[]> _resourceCache = [];

    /// <summary>
    /// Loads a resource file with the given <paramref name="resourceName"/>.
    /// </summary>
    /// <param name="resourceName">Resource file name.</param>
    /// <returns>The text contained in the loaded file.</returns>
    protected string LoadEmbeddedStringResource(string resourceName)
    {
        using var stream = GetEmbeddedResourceStream(resourceName);

        // Read the content of the resource
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Loads a resource file with the given <paramref name="resourceName"/>.
    /// </summary>
    /// <param name="resourceName">Resource file name.</param>
    /// <returns>The binary data contained in the loaded file.</returns>
    protected byte[] LoadEmbeddedBinaryResource(string resourceName)
    {
        if (_resourceCache.ContainsKey(resourceName))
        {
            return _resourceCache[resourceName];
        }

        using var stream = GetEmbeddedResourceStream(resourceName);

        // Read the content of the resource
        using var reader = new MemoryStream();
        stream.CopyTo(reader);
        var data = reader.ToArray();
        _resourceCache[resourceName] = data;
        return data;
    }

    private Stream GetEmbeddedResourceStream(string resourceName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var resource = GetEmbeddedResourceStream(resourceName, assembly);
            if (resource != null) return resource;
        }

        throw new FileNotFoundException($"Cannot find resource {_resourceLocation}.{resourceName}");
    }

    private Stream? GetEmbeddedResourceStream(string resourceName, Assembly assembly)
    {
        // Get all resource names in the assembly
        var resourceNames = assembly.GetManifestResourceNames();

        // Find a matching resource name (case-insensitive comparison)
        var match = resourceNames.FirstOrDefault(r => r.EndsWith($"{_resourceLocation}.{resourceName}", StringComparison.OrdinalIgnoreCase));
        if (match == null) return null;

        // Load the resource stream
        return assembly.GetManifestResourceStream(match);
    }
}
