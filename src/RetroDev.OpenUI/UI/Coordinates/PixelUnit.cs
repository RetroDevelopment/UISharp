using System.Diagnostics;
using System.Numerics;

namespace RetroDev.OpenUI.UI.Coordinates;

using ValueType = float;

[DebuggerDisplay("{ToString()}")]
public record class PixelUnit : IEquatable<PixelUnit>, IComparable<PixelUnit>
{
    public static readonly PixelUnit Auto = float.NaN;
    public static readonly PixelUnit Zero = 0.0f;

    public ValueType Value { get; }

    public bool IsAuto => ValueType.IsNaN(Value);

    public PixelUnit(ValueType valueType = 0) { Value = valueType; }

    public static implicit operator PixelUnit(ValueType value) => new(value);
    public static implicit operator ValueType(PixelUnit coordinate) => coordinate.Value;
    public static PixelUnit operator -(PixelUnit a, PixelUnit b) => a.Value - b.Value;

    public override string ToString() => IsAuto ? "auto" : Value.ToString();

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public int CompareTo(PixelUnit? other) => (int)(Value - other?.Value ?? 0.0f);
}
