using RetroDev.UISharp.UIDefinition;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// Represents a relative size as a percentage (e.g., "1.01%").
/// </summary>
[EAMLMatch(@"^(?<size>\d+(\.\d+)?)%$")]
public record RelativeSize(float Size) : IGridSize;
