
namespace RetroDev.OpenUI.Components.Containers;

public abstract class Container(Application application) : UIComponent(application), IGenericContainer
{
    public abstract IEnumerable<UIComponent> Children { get; }

    /// <inheritdoc />
    protected override bool DefaultIsFocusable => false;

    /// <summary>
    /// Gets the child component with <see cref="ID"/> equal to the given <paramref name="id"/>.
    /// </summary>
    /// <typeparam name="TComponent">The comnponent type.</typeparam>
    /// <returns>The component.</returns>
    /// <exception cref="ArgumentException">If the component does not exist.</exception>
    /// <exception cref="InvalidCastException">If the component was found but with a type not assignable to <typeparamref name="TComponent"/>.</exception>
    public virtual TComponent GetComponent<TComponent>(string id) where TComponent : UIComponent
    {
        var children = Children.Where(c => c.ID == id);
        if (!children.Any()) throw new ArgumentException($"Child with ID {id} not found in component with id {ID.Value}");
        return (TComponent)children.First();
    }
}
