namespace RetroDev.OpenUI.Components;

/// <summary>
/// Specify OpenIDE editor settings for a <see cref="UIComponent"/>.
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
