namespace RetroDev.OpenUI.Core;

public interface IWindowId : IEquatable<IWindowId>
{
    int Id { get; }
}
