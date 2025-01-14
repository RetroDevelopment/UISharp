namespace RetroDev.OpenUI.Components.AutoArea;

/// <summary>
/// Contains all the pre-defined auto <see cref="IHorizontalAlignment"/> and <see cref="IVerticalAlignment"/>.
/// </summary>
public static class Alignment
{
    /// <summary>
    /// Left alignment.
    /// </summary>
    public static readonly IHorizontalAlignment Left = new Left();

    /// <summary>
    /// Right alignment.
    /// </summary>
    public static readonly IHorizontalAlignment Right = new Right();

    /// <summary>
    /// Top alignment.
    /// </summary>
    public static readonly IVerticalAlignment Top = new Top();

    /// <summary>
    /// Bottom alignment.
    /// </summary>
    public static readonly IVerticalAlignment Bottom = new Bottom();


    /// <summary>
    /// Center alignment.
    /// </summary>
    public static readonly IAlignment Center = new Center();
}
