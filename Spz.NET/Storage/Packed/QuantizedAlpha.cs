using System.Runtime.CompilerServices;
using Spz.NET.Helpers;

namespace Spz.NET;

public readonly struct QuantizedAlpha(float alpha)
{
    public readonly float Alpha => SplatMathHelpers.InvSigmoid(AlphaQ / 255f);

    public readonly byte AlphaQ = (SplatMathHelpers.Sigmoid(alpha) * 255f).ByteClamp();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuantizedAlpha(float other) => new(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(QuantizedAlpha other) => other.Alpha;
}
