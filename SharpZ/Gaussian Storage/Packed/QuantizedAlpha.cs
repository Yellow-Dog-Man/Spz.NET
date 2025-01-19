using System.Runtime.CompilerServices;

namespace SharPZ;

public readonly struct QuantizedAlpha
{
    public readonly float Alpha => SplatMathHelpers.InvSigmoid(AlphaQ / 255f);

    public readonly byte AlphaQ;

    public QuantizedAlpha(float alpha)
    {
        AlphaQ = (byte)(SplatMathHelpers.Sigmoid(alpha) * 255f);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuantizedAlpha(float other) => new(other);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator float(QuantizedAlpha other) => other.Alpha;
}
