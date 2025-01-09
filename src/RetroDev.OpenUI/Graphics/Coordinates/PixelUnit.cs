using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

using ValueType = float;

[DebuggerDisplay("{ToString()}")]
public record class PixelUnit : IEquatable<PixelUnit>
{
    public static readonly PixelUnit Auto = float.NaN;

    public ValueType Value { get; }

    public bool IsAuto => ValueType.IsNaN(Value);

    public PixelUnit(ValueType valueType = 0) { Value = valueType; }

    public static implicit operator PixelUnit(ValueType value) => new(value);
    public static implicit operator ValueType(PixelUnit coordinate) => coordinate.Value;

    public override string ToString() => IsAuto ? "auto" : Value.ToString();

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
