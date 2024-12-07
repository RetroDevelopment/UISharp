using System.Diagnostics;

namespace RetroDev.OpenUI.Core.Coordinates;

using ValueType = float;

[DebuggerDisplay("{ToString()}")]
public class PixelUnit : IEquatable<PixelUnit>
{
    public static readonly PixelUnit Auto = float.NaN;

    public ValueType Value { get; set; }

    public bool IsAuto => ValueType.IsNaN(Value);

    public PixelUnit(ValueType valueType) { Value = valueType; }

    public bool Equals(PixelUnit? other)
    {
        return Value == other?.Value;
    }

    public static implicit operator PixelUnit(ValueType value) => new(value);
    public static implicit operator ValueType(PixelUnit coordinate) => coordinate.Value;

    public override bool Equals(object obj) => Equals(obj as PixelUnit);

    public override string ToString() => IsAuto ? "auto" : Value.ToString();

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}
