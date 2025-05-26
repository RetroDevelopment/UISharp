using System;
using System.Text.RegularExpressions;

namespace RetroDev.UISharp.UIDefinition;

/// <summary>
/// Attribute that defines a regex pattern for matching instances of a class.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class EAMLMatchAttribute : Attribute
{
    /// <summary>
    /// The regex pattern used for matching.
    /// </summary>
    public Regex Pattern { get; }

    /// <summary>
    /// Initializes the attribute with a regex pattern for matching instances.
    /// </summary>
    /// <param name="pattern">The regex pattern used for matching.</param>
    public EAMLMatchAttribute(string pattern)
    {
        Pattern = new Regex(pattern, RegexOptions.Compiled);
    }
}
