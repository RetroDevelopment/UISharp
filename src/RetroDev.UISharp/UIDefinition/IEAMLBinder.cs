using System.Reflection;
using RetroDev.UISharp.Presentation.Properties;
using RetroDev.UISharp.UIDefinition.Exceptions;
using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.UIDefinition;

/// <summary>
/// The common interface to implement EAML, the Extensible Attribute Mapping Language.
/// The EAML langauge allows binding a UIDefinition xml attribute with the actual property value and it is fully
/// extensible by extending the <see cref="EAMLBinder"/> and passing it to the <see cref="InstanceCreator"/> or <see cref="UIDefinitionManager"/>,
/// or it can be rewritten from scratch implementing this interface.
/// This means that it is possible to fully customize how to bind a xml attriute to the actual <see cref="UIProperty{TParent, TValue}"/>.
/// </summary>
public interface IEAMLBinder
{
    /// <summary>
    /// A list of variables that can be used in the attribute mapping. This can be used for binding.
    /// </summary>
    Dictionary<string, object> Instances { get; set; }

    /// <summary>
    /// Sets the bindable property defined by the given <paramref name="propertyInfo"/> to the
    /// value defined by the given <paramref name="attribute"/>.
    /// </summary>
    /// <param name="propertyInfo">The property set. Its type must be either <see cref="UIProperty{TValue}"/> or <see cref="UICompositeProperty{TValue}"/>.</param>
    /// <param name="attribute">The attribute definition.</param>
    /// <param name="componentInstance">The <see cref="UIComponent"/> owning the property defined by <paramref name="propertyInfo"/>.</param>
    /// <exception cref="UIDefinitionValidationCompoundException">If something failes during the property assignment.</exception>
    /// <exception cref="InvalidOperationException">If the given <paramref name="propertyInfo"/> is not a valid bindable property.</exception>
    void SetGenericUIProperty(PropertyInfo propertyInfo, Ast.Attribute attribute, UIComponent componentInstance);
}
