using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.Components.Core;

/// <summary>
/// Specify UISharp IDE editor settings for a <see cref="UIObject"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class EditorSettingsAttribute : Attribute
{
    public bool Allow { get; }

    public EditorSettingsAttribute(bool allow)
    {
        Allow = allow;
    }
}
