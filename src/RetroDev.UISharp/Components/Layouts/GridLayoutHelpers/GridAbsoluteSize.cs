using RetroDev.UISharp.Core.Coordinates;
using RetroDev.UISharp.UIDefinition;

namespace RetroDev.UISharp.Components.Layouts.GridLayoutHelpers;

/// <summary>
/// Represents an absolute size in pixels (e.g., "100px", "100.123px").
/// </summary>
[EAMLMatch(@"^(?<size>\d+(\.\d+)?)px$")]
public record GridAbsoluteSize(PixelUnit Size) : IGridSize;
