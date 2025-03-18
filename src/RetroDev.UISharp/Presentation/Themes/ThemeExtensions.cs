using RetroDev.UISharp.Components.Base;
using RetroDev.UISharp.Core.Graphics;
using RetroDev.UISharp.Presentation.Properties;

namespace RetroDev.UISharp.Presentation.Themes;

/// <summary>
/// Contains theme related extension methods.
/// </summary>
public static class ThemeExtensions
{
    /// <summary>
    /// Binds <paramref name="this"/> <see cref="UIProperty{TValue}"/> to the color theme with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="this">The property to bind.</param>
    /// <param name="id">The color id.</param>
    public static void BindTheme(this UIProperty<Color> @this, string id)
    {
        var property = @this.Application.ThemeManager.GetColorProperty(id);
        @this.BindSourceToDestination(property);
    }

    /// <summary>
    /// Binds <paramref name="this"/> <see cref="UIProperty{TValue}"/> to the color theme with the given <paramref name="id"/>.
    /// </summary>
    /// <param name="this">The property to bind.</param>
    /// <param name="id">The color id.</param>
    /// <param name="converter">The function converting the theme <see cref="Color"/> to a value of type <typeparamref name="TSource"/>.</param>
    /// <typeparam name="TSource"><paramref name="this"/> <see cref="UIProperty{TValue}"/> value type.</typeparam>
    public static void BindTheme<TSource>(this UIProperty<TSource> @this, string id, Func<Color, TSource> converter)
    {
        var property = @this.Application.ThemeManager.GetColorProperty(id);
        @this.BindSourceToDestination(property, converter);
    }
}
