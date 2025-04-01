using System.Reflection;
using System.Text.RegularExpressions;
using RetroDev.UISharp.Components.Core.Base;
using RetroDev.UISharp.Core.Logging;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.UIDefinition.Exceptions;

// TODO: bindings
// {dictionaryKey.property}
// {dictionaryKey.property, TwoWays, MyConverter(...)}
// or
// {dictionaryKey.property, TwoWays, converter} -> maybe pass a converter as instance to re use it
// or
// {dictionaryKey.Action()} -> invoke a method from the object. But do not pass any argument (at lest for now)

// Examples:
// <editText text="{this.Name, TwoWays, IdentityConverter()}" /> -> or exceptionally for "this" object
// <editText text="{Name}" /> -> and TwoWays is automatic (or SourceToDestination if TwoWays is not supported, etc.)
// <button action="{SaveFile()}" />
// <treeBox items="{Employees, ItemTemplate('employee.xml')}" />

namespace RetroDev.UISharp.UIDefinition;

/// <summary>
/// The implementation of EAML, the Extensible Attribute Mapping Language.
/// The EAML language allows binding a UIDefinition xml attribute with the actual property value and it is fully
/// extensible by extending this class and passing it to the <see cref="InstanceCreator"/> or <see cref="UIDefinitionManager"/>,
/// or it can be rewritten from scratch implementing the <see cref="IEAMLBinder"/> interface.
/// This means that it is possible to fully customize how to bind a xml attribute to the actual <see cref="UIProperty{TParent, TValue}"/>.
/// </summary>
/// <remarks>
/// Creates a new EAML binder.
/// </remarks>
/// <param name="typeMapper">
/// The object that find the relevant UI types defined in the UISharp as well as the types defined in the project using UISharp.
/// </param>
public class EAMLBinder(TypeMapper typeMapper) : IEAMLBinder
{
    /// <summary>
    /// A list of variables that can be used in the attribute mapping. This can be used for binding.
    /// </summary>
    public Dictionary<string, object> Instances { get; set; } = [];

    /// <summary>
    /// Sets the UI property defined by the given <paramref name="propertyInfo"/> to the
    /// value defined by the given <paramref name="attribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property set. Its type must be either <see cref="UIProperty{TValue}"/> or <see cref="UICompositeProperty{TValue}"/>.</param>
    /// <param name="attribute">The attribute definition.</param>
    /// <param name="componentInstance">The <see cref="UIComponent"/> owning the property defined by <paramref name="propertyInfo"/>.</param>
    /// <exception cref="UIDefinitionValidationCompoundException">If something fails during the property assignment.</exception>
    /// <exception cref="InvalidOperationException">If the given <paramref name="propertyInfo"/> is not a valid bindable property.</exception>
    public virtual void SetGenericUIProperty(PropertyInfo propertyInfo, Ast.Attribute attribute, UIComponent componentInstance)
    {
        if (propertyInfo.IsUIProperty())
        {
            SetUIProperty(propertyInfo, attribute, componentInstance);
        }
        else if (propertyInfo.IsUICompositeProperty())
        {
            SetUICompositeProperty(propertyInfo, attribute, componentInstance);
        }
        else if (propertyInfo.IsUIPropertyCollection())
        {
            SetUIPropertyCollection(propertyInfo, attribute, componentInstance);
        }
        else
        {
            throw new InvalidOperationException($"Invalid UI property {propertyInfo}");
        }
    }

    /// <summary>
    /// Sets the <see cref="UIProperty{TValue}"/> property defined by the given <paramref name="propertyInfo"/> to the
    /// value defined by the given <paramref name="attribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property set. Its type must be either <see cref="UIProperty{TValue}"/>.</param>
    /// <param name="attribute">The attribute definition.</param>
    /// <param name="componentInstance">The instance owning the property defined by <paramref name="propertyInfo"/>.</param>
    /// <exception cref="UIDefinitionValidationCompoundException">If something fails during the property assignment.</exception>
    /// <exception cref="InvalidOperationException">If the given <paramref name="propertyInfo"/> is not a valid bindable property.</exception>
    protected virtual void SetUIProperty(PropertyInfo propertyInfo, Ast.Attribute attribute, object componentInstance)
    {
        var propertyValueType = propertyInfo.PropertyType;
        var propertyValue = propertyInfo.GetValue(componentInstance) ?? throw new UIDefinitionValidationException("Property value cannot be null", attribute);
        RemoveBindingsBeforeAssignment(propertyValueType, propertyValue, attribute);
        var propertyValueInfo = propertyValue.GetType().GetProperty("Value") ?? throw new UIDefinitionValidationException("Property value Value inner property not found", attribute);
        var propertyType = propertyValueInfo.PropertyType;
        var actualValue = ConvertAttributeValueToPropertyValue(attribute, propertyType);
        propertyValueInfo.SetValue(propertyValue, actualValue);
    }

    /// <summary>
    /// Sets the <see cref="UICompositeProperty{TValue}"/> property defined by the given <paramref name="propertyInfo"/> to the
    /// value defined by the given <paramref name="attribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property set. Its type must be either <see cref="UICompositeProperty{TValue}"/>.</param>
    /// <param name="attribute">The attribute definition.</param>
    /// <param name="componentInstance">The <see cref="UIComponent"/> owning the property defined by <paramref name="propertyInfo"/>.</param>
    /// <exception cref="UIDefinitionValidationCompoundException">If something fails during the property assignment.</exception>
    /// <exception cref="InvalidOperationException">If the given <paramref name="propertyInfo"/> is not a valid bindable property.</exception>
    protected virtual void SetUICompositeProperty(PropertyInfo propertyInfo, Ast.Attribute attribute, UIComponent componentInstance)
    {
        var propertyValueType = propertyInfo.PropertyType;
        var propertyInfos = propertyValueType.GetUIProperties();
        var attributes = ToAttributeList(attribute.Value);
        var propertyValue = propertyInfo.GetValue(componentInstance) ?? throw new UIDefinitionValidationException("Property value cannot be null", attribute);

        if (attributes.Count == 1) attributes = Enumerable.Repeat(attributes.First(), propertyInfos.Count).ToList();
        if (attributes.Count != propertyInfos.Count) throw new UIDefinitionValidationException($"Compound property expects {propertyInfos.Count} values but {attributes.Count} values where provided", attribute);
        var count = propertyInfos.Count;

        for (int i = 0; i < count; i++)
        {
            var compositeAttribute = attributes[i];
            var compositePropertyInfo = propertyInfos[i];
            SetUIProperty(compositePropertyInfo, new Ast.Attribute($"{attribute.Name}.{compositePropertyInfo.Name}", compositeAttribute), propertyValue);
        }
    }

    /// <summary>
    /// Sets the <see cref="UIPropertyCollection{TValue}"/> property defined by the given <paramref name="propertyInfo"/> to the
    /// value defined by the given <paramref name="attribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property set. Its type must be either <see cref="UIPropertyCollection{TValue}"/>.</param>
    /// <param name="attribute">The attribute definition.</param>
    /// <param name="componentInstance">The <see cref="UIComponent"/> owning the property defined by <paramref name="propertyInfo"/>.</param>
    /// <exception cref="UIDefinitionValidationCompoundException">If something failes during the property assignment.</exception>
    /// <exception cref="InvalidOperationException">If the given <paramref name="propertyInfo"/> is not a valid bindable property.</exception>
    protected virtual void SetUIPropertyCollection(PropertyInfo propertyInfo, Ast.Attribute attribute, UIComponent componentInstance)
    {
        var propertyValueType = propertyInfo.PropertyType;
        var propertyValue = propertyInfo.GetValue(componentInstance) ?? throw new UIDefinitionValidationException("Property value cannot be null", attribute);
        RemoveBindingsBeforeAssignment(propertyValueType, propertyValue, attribute);
        var propertyType = propertyValueType.GetGenericArguments()[0];
        var attributes = ToAttributeList(attribute.Value);
        var addMethodName = nameof(UIPropertyCollection<object>.Add);
        var addMethod = propertyValueType.GetMethod(addMethodName);
        if (addMethod is null) throw new ArgumentException($"Cannot find {addMethodName} method in type {propertyValueType}");
        var i = 0;

        foreach (var attributeStringValue in attributes)
        {
            var attributeName = $"{attribute.Name}[{i}]";
            var actualValue = ConvertAttributeValueToPropertyValue(new Ast.Attribute(attributeName, attributeStringValue), propertyType);
            addMethod.Invoke(propertyValue, [actualValue]);
            i++;
        }
    }

    /// <summary>
    /// Converts the given AST <paramref name="attribute"/> to an <see cref="object"/>.
    /// </summary>
    /// <param name="attribute">The attribute AST, containing the text to convert into a value.</param>
    /// <param name="propertyType">The target property type to assign the converted attribute object.</param>
    /// <returns>
    /// An instance of <see cref="object"/> representing the converted attribute.
    /// The converted object might be <see langword="null" />.
    /// </returns>
    /// <exception cref="UIDefinitionValidationException">If the conversion fails.</exception>
    protected virtual object? ConvertAttributeValueToPropertyValue(Ast.Attribute attribute, Type propertyType)
    {
        object? value;
        if (TryConvertAttributeToPrimitiveType(out value, attribute, propertyType)) return value;
        if (TryConvertAttributeUsingConstructor(out value, attribute, propertyType)) return value;
        if (TryConvertAttributeUsingInterfaceHierarchy(out value, attribute, propertyType)) return value;
        if (TryConvertAttributeUsingEnums(out value, attribute, propertyType)) return value;

        throw new UIDefinitionValidationException($"Failed to convert {attribute.Name}={attribute.Value}", attribute);
    }

    /// <summary>
    /// Try to convert the given <paramref name="attribute"/> into a primitive type, such as, <see cref="int"/> or <see cref="string"/>.
    /// This is possible if the target <paramref name="propertyType"/> is a <see cref="UIProperty{TParent, TValue}"/> where the value type is a primitive type.
    /// </summary>
    /// <param name="value">The conversion result.</param>
    /// <param name="attribute">The attribute to convert.</param>
    /// <param name="propertyType">The target property type.</param>
    /// <exception cref="UIDefinitionValidationException">If the conversion failed with an error.</exception>
    /// <returns><see langword="true" /> if it is possible to convert the attribute into a primitive type, otherwise <see langword="false" /></see></returns>
    protected virtual bool TryConvertAttributeToPrimitiveType(out object? value, Ast.Attribute attribute, Type propertyType)
    {
        try
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(propertyType);
            if (converter is not null && converter.IsValid(attribute.Value))
            {
                value = converter.ConvertFromInvariantString(attribute.Value);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }
        catch (Exception e)
        {
            throw new UIDefinitionValidationException($"Failed convert value '{attribute.Value}' to type {propertyType.Name}: conversion exception", attribute, e);
        }
    }

    /// <summary>
    /// Try to convert the given <paramref name="attribute"/> into an object of the same type as the given <paramref name="propertyType"/> type.
    /// </summary>
    /// <param name="value">The conversion result.</param>
    /// <param name="attribute">The attribute to convert.</param>
    /// <param name="propertyType">The target property type.</param>
    /// <exception cref="UIDefinitionValidationException">If the conversion failed with an error.</exception
    /// <example>
    /// If <paramref name="propertyType"/> is a class <c>C</c> with constructor <c>C(double)</c>
    /// attribute="100.0"
    /// value=new C(100.0)
    /// </example>
    /// <example>
    /// If <paramref name="propertyType"/> is a class <c>C</c> with constructor <c>C(int, double)</c>
    /// attribute="(10, 100.0)"
    /// value=new C(10, 100.0)
    /// </example>
    /// <example>
    /// If <paramref name="propertyType"/> is a class <c>C</c> with constructor <c>C(int, double)</c> and it defines a readonly static variable <c>C.Constant</c>
    /// attribute="(Constant, 100.0)"
    /// value=new C(C.Constant, 100.0)
    /// </example>
    /// <returns><see langword="true" /> if it is possible to convert the attribute into a primitive type, otherwise <see langword="false" /></see></returns>
    protected virtual bool TryConvertAttributeUsingConstructor(out object? value, Ast.Attribute attribute, Type propertyType)
    {
        if (propertyType.GetConstructors().Length == 0 || propertyType.IsAbstract)
        {
            value = null;
            return false;
        }

        if (TryFindReadonlyStaticFieldFromType(out value, propertyType, attribute.Value)) return true;

        var attributeArguments = FindAttributeArguments(attribute);
        var constructor = FindConstructor(propertyType, attributeArguments.Count, attribute);

        List<object?> arguments = [];
        var i = 0;
        foreach (var attributeArgument in attributeArguments)
        {
            arguments.Add(ConvertAttributeArgumentToConstructorArgument(attributeArgument, constructor, i, attribute));
            i++;
        }

        value = constructor.Invoke([.. arguments]);
        return true;
    }

    /// <summary>
    /// Try to convert the given <paramref name="attribute"/> into an object of the same type as the given <paramref name="propertyType"/> type,
    /// assuming <paramref name="propertyType"/> is an interface type. If the <paramref name="attribute"/> text is <c>MyType</c>, <c>new MyType()</c> will be
    /// returned assuming that a class with name <c>MyType</c> with empty constructor implementing the <paramref name="propertyType"/> interface exists.
    /// </summary>
    /// <param name="value">The conversion result.</param>
    /// <param name="attribute">The attribute to convert.</param>
    /// <param name="propertyType">The target property type.</param>
    /// <exception cref="UIDefinitionValidationException">If the conversion failed with an error.</exception
    /// <example>
    /// Given an interface <c>I</c> and a class <c>C</c> implementing <c>I</c>, then <br />
    /// attribute="C" <br />
    /// value=new C() <br />
    /// </example>
    /// <example>
    /// Given an interface <c>I</c>, a class <c>C</c> using the <see cref="EAMLMatchAttribute"/> with a regex <c>(?&lt;unit&gt;\d+)units</c>, then <br />
    /// attribute="100units" <br />
    /// value=new C(100)
    /// if C is defined as C(int unit) where using can have any numeric type.
    /// Note that only regular expression matches with non-numeric named capture group are considered and matched with parameters.
    /// In the example above, ?&lt;unit&gt; names the respective capture group and the name is used to match the <c>C</c> constructor parameter with the same name: <c>unit</c>.
    /// Capture group names must all be lower case.
    /// </example>
    /// <returns><see langword="true" /> if it is possible to convert the attribute into a primitive type, otherwise <see langword="false" /></see></returns>
    protected virtual bool TryConvertAttributeUsingInterfaceHierarchy(out object? value, Ast.Attribute attribute, Type propertyType)
    {
        if (!propertyType.IsInterface)
        {
            value = null;
            return false;
        }

        var implementationTypes = typeMapper
            .GetAllTypes()
            .Where(type => !type.IsAbstract && type.IsAssignableTo(propertyType));

        Type? matchedType = null;
        Dictionary<string, string> matches = [];

        foreach (var type in implementationTypes)
        {
            var regexMatches = Match(attribute.Value, type);
            var typeNameMatches = string.Equals(type.Name, attribute.Value, StringComparison.OrdinalIgnoreCase);
            if (regexMatches is not null || typeNameMatches)
            {
                matchedType = type;
                matches = regexMatches ?? [];
            }
        }

        if (matchedType is null) throw new UIDefinitionValidationException($"Cannot find type {attribute.Value} to match attribute {attribute.Name}. A type with that name assignable to {propertyType} does not exist.", attribute);
        var matchedConstructors = matchedType.GetConstructors().Where(c => c.GetParameters().Length == matches.Count);
        if (!matchedConstructors.Any()) throw new UIDefinitionValidationException($"Cannot find empty constructor for type {matchedType}, needed to instantiate {attribute.Name}={attribute.Value}", attribute);
        if (matchedConstructors.Count() > 1) throw new UIDefinitionValidationException($"More than one constructor with {matches.Count} parameters found. Only one constructor with the same number of parameters as regex matches must be specified as multiple constructor matching is not supported.", attribute);
        var constructor = matchedConstructors.First();

        value = constructor.Invoke(CreateConstructorArgument(constructor, matches, attribute));
        return true;
    }

    /// <summary>
    /// Try to convert the given <paramref name="attribute"/> into an object of the same type as the given <paramref name="propertyType"/> type,
    /// assuming <paramref name="propertyType"/> is an enum type.
    /// </summary>
    /// <param name="value">The conversion result.</param>
    /// <param name="attribute">The attribute to convert.</param>
    /// <param name="propertyType">The target property type.</param>
    /// <exception cref="UIDefinitionValidationException">If the conversion failed with an error.</exception
    /// <returns><see langword="true" /> if it is possible to convert the attribute into a primitive type, otherwise <see langword="false" /></see></returns>
    protected virtual bool TryConvertAttributeUsingEnums(out object? value, Ast.Attribute attribute, Type propertyType)
    {
        if (!propertyType.IsEnum)
        {
            value = null;
            return false;
        }

        value = Enum.Parse(propertyType, attribute.Value, ignoreCase: true);
        return true;
    }

    private List<string> FindAttributeArguments(Ast.Attribute attribute)
    {
        string pattern = @"[^,\s]+";
        var attributeArguments = Regex.Matches(attribute.Value, pattern)
                             .Select(m => m.Value)
                             .ToList();
        return attributeArguments;
    }

    private ConstructorInfo FindConstructor(Type type, int argumentCount, Ast.Attribute attribute)
    {
        var constructor = type.GetConstructors()
                              .Where(c => c.GetParameters().Length == argumentCount)
                              .FirstOrDefault();
        if (constructor is null) throw new UIDefinitionValidationException($"Failed to find a suitable constructor for attribute {attribute.Name}={attribute.Value}", attribute);

        return constructor;
    }

    private object? ConvertAttributeArgumentToConstructorArgument(string attributeArgument, ConstructorInfo constructor, int position, Ast.Attribute attribute)
    {
        object? argument;
        var attributeParameter = new Ast.Attribute($"{attribute.Name}#{position}", attributeArgument);
        if (TryFindReadonlyStaticFieldFromType(out argument, constructor.GetParameters()[position].ParameterType, attributeArgument)) return argument;
        var conversionSuccess = TryConvertAttributeToPrimitiveType(out argument, attribute, constructor.GetParameters()[position].ParameterType);
        if (!conversionSuccess) throw new UIDefinitionValidationException($"Failed to match argument {position} ({attributeArgument} to constructor {constructor})", attribute);
        return argument;
    }

    private bool TryFindReadonlyStaticFieldFromType(out object? value, Type type, string fieldName)
    {
        var fieldInfo = type.GetField(fieldName,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.IgnoreCase);

        if (fieldInfo == null || !fieldInfo.IsInitOnly)
        {
            value = null;
            return false;
        }

        value = fieldInfo.GetValue(null);
        return true;
    }

    private void RemoveBindingsBeforeAssignment(Type propertyValueType, object propertyValue, Ast.Attribute attribute)
    {
        var removeBindersName = nameof(UIProperty<object>.Unbind);
        var removeBindingsMethod = propertyValueType.GetMethod(removeBindersName) ?? throw new UIDefinitionValidationException($"Missing method {removeBindersName} in property type {propertyValueType.FullName}", attribute);
        removeBindingsMethod.Invoke(propertyValue, []);
    }

    private Dictionary<string, string>? Match(string value, Type type)
    {
        var attribute = type.GetCustomAttribute<EAMLMatchAttribute>();
        if (attribute is null) return null;
        var match = attribute.Pattern.Match(value);
        if (!match.Success) return null;
        return match
            .Groups
            .Cast<Group>()
            .Skip(1)
            .Where(group => !int.TryParse(group.Name, out _)) // Skip numeric keys (unnamed groups)
            .Select(group => KeyValuePair.Create(group.Name, group.Value))
            .ToDictionary();
    }

    private object[] CreateConstructorArgument(ConstructorInfo constructor, Dictionary<string, string> matches, Ast.Attribute attribute)
    {
        var result = new List<object>();
        var i = 0;

        foreach (var constructorParameter in constructor.GetParameters())
        {
            var type = constructorParameter.ParameterType;
            if (constructorParameter.Name is null) throw new ArgumentException($"Parameter name null in constructor {constructor}: make sure no optimization or code obfuscation is enabled for the class.");
            var stringValue = matches[constructorParameter.Name.ToLower()];
            var value = ConvertAttributeValueToPropertyValue(new Ast.Attribute($"{attribute.Name}.{constructorParameter.Name}", stringValue), type);
            if (value is null) throw new UIDefinitionValidationException($"Failed to assign value {attribute.Value} for attribute {attribute.Name} to constructor parameter {constructorParameter.Name}", null);
            result.Add(value);
            i++;
        }

        return result.ToArray();
    }

    private IReadOnlyList<string> ToAttributeList(string attribute) =>
        attribute.Split([';']);
}
