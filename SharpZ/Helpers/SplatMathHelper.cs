using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharPZ;

public static class SplatMathHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
        return Math.Min(Math.Max(min, value), max);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte QuantizeSH(float x, int bucketSize)
    {
        int q = (int)(Math.Round(x * 128f) + 128f);

        q = (q + bucketSize / 2) / bucketSize * bucketSize;
        return (byte)Clamp(q, 0, 255);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float UnquantizeSH(byte x) => (x - 128f) / 128f;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PackedCoefficient QuantizeCoefficient(this Vector3 x, int bucketSize) => new(QuantizeSH(x.X, bucketSize), QuantizeSH(x.Y, bucketSize), QuantizeSH(x.Z, bucketSize));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 UnquantizeCoefficient(this PackedCoefficient x) => new(UnquantizeSH(x.X), UnquantizeSH(x.Y), UnquantizeSH(x.Z));



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sigmoid(float x) => 1f / (1f + (float)Math.Exp(-x));

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InvSigmoid(float x) => (float)Math.Log(x / (1f - x));
}


public static class FixedPointHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed24 ToFixed(this float value, int fractionalBits) => new(value, fractionalBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedVector3 ToFixed(this Vector3 value, int fractionalBits) => new(value, fractionalBits);
}