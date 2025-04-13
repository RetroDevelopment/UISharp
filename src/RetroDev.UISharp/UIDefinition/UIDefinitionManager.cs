using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.UIDefinition;

/// <summary>
/// This is the main class to parse and compile UIDefinition language, which is an xml-based language to define UI in
/// UISharp.
/// </summary>
public class UIDefinitionManager
{
    /// <summary>
    /// The application that owns <see langword="this" /> object.
    /// </summary>
    public Application Application { get; }

    /// <summary>
    /// The object that find the relevant UI types defined in the UISharp as well as the types defined in the project using UISharp.
    /// </summary>
    public TypeMapper TypeMapper { get; }


    /// <summary>
    /// The UIDefinition language parser.
    /// </summary>
    public Parser Parser { get; }

    /// <summary>
    /// The object that creates UI objects (windows, button, etc.) from the UIDefinition langague AST tree.
    /// </summary>
    public InstanceCreator InstanceCreator { get; }

    /// <summary>
    /// The object that generates UIDefinitionLanguage xml tetxt from the UIDefinition language AST tree.
    /// </summary>
    public CodeGenerator CodeGenerator { get; }

    /// <summary>
    /// Creats a new <see cref="UIDefinitionManager"/>.
    /// </summary>
    /// <param name="application">The application that owns <see langword="this" /> object.</param>
    /// <param name="binder">
    /// The binder to define the EAML language, which binds UIDefinition attributes with actual values.
    /// If this parameter is not specified, a <see cref="EAMLBinder"/> isntance will be created, using the default EAML implementation.
    /// </param>
    public UIDefinitionManager(Application application, IEAMLBinder? binder = null)
    {
        Application = application;
        TypeMapper = new TypeMapper();
        Parser = new Parser();
        InstanceCreator = new InstanceCreator(Application, TypeMapper, binder);
        CodeGenerator = new CodeGenerator();
    }

    /// <summary>
    /// Creates the <see cref="UIObject"/> instance as defined in the given UIDefinition <paramref name="xml"/> text.
    /// </summary>
    /// <param name="xml">The UIDefinition xml containing the instance to create.</param>
    /// <returns>The comonent instance as defined in the given <paramref name="xml"/>.</returns>
    public UIObject CreateUIComponent(string xml)
    {
        var parser = new Parser();
        var validator = new Validator(TypeMapper);
        var componentDefinition = parser.Parse(xml);
        validator.Validate(componentDefinition);
        return InstanceCreator.CreateUIComponent(componentDefinition);
    }
}
