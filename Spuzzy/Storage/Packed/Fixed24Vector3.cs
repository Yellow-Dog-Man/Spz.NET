using System.Numerics;
using System.Runtime.CompilerServices;

namespace Spuzzy;

/// <summary>
/// An analog of <see cref="Vector3"/> with each component as a 24-bit fixed-point number with a given number of bits dedicated to the fractional portion.
/// </summary>
/// <param name="value">The vector to be converted to a fixed-point representation</param>
/// <param name="fractionalBits">The number of bits dedicated to representing the fractional portion of each component.</param>
public readonly struct FixedVector3(in Vector3 value, int fractionalBits)
{
    public readonly Fixed24 X = value.X.ToFixed(fractionalBits);
    public readonly Fixed24 Y = value.Y.ToFixed(fractionalBits);
    public readonly Fixed24 Z = value.Z.ToFixed(fractionalBits);



    public Vector3 ToVector3(int fractionalBits) => new(X.ToFloat(fractionalBits), Y.ToFloat(fractionalBits), Z.ToFloat(fractionalBits));
}
