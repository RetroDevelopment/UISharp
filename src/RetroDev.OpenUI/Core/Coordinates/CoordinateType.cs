using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

[DebuggerDisplay("{Value}")]
public class CoordinateType
{
    public int Value { get; set; }

    public override string ToString() => Value.ToString();

    public static implicit operator CoordinateType(int value) => new() { Value = value };
    public static implicit operator int(CoordinateType coordinate) => coordinate.Value;
}
