using RetroDev.UISharp.UIDefinition;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// Represents an automatic size (e.g., "*" or "auto").
/// </summary>
[EAMLMatch(@"^(\*|auto)$")]
public record GridAutoSize : IGridSize;
