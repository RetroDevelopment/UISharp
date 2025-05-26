using RetroDev.UISharp.Components.Core.Base;

namespace RetroDev.UISharp.Components.Collections.DropDownHelpers;

/// <summary>
/// Creates new <see cref="UIObject"/> instances from an existing one to display
/// preview. For example, in a dropdown list, it creates a new item that displays the preview
/// of the selected <see cref="UIObject"/>.
/// This interface allows custom implementation for the item preview rendering logics: allowing
/// item preview to be different from the item in a dropdown list.
/// </summary>
public interface IPreviewRenderer
{
    /// <summary>
    /// Creates a clone of the given <paramref name="item"/>.
    /// </summary>
    /// <typeparam name="TObject">The <paramref name="item"/> type.</typeparam>
    /// <param name="item">The item to clone.</param>
    /// <returns>A new <see cref="UIObject"/> that represents the original <paramref name="item"/>.</returns>
    TObject Clone<TObject>(TObject item) where TObject : UIObject;

    /// <summary>
    /// Removes bindings from the given <paramref name="item"/>.
    /// </summary>
    /// <param name="item">The <see cref="UIObject"/> from which to remove bindings.</param>
    void Unbind(UIObject item);
}
