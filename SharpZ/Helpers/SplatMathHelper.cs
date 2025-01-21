using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharPZ;

public static class SplatMathHelpers
{
    const int SH1_BUCKET_SIZE = 1 << (8 - 5);
    const int SHREST_BUCKET_SIZE = 1 << (8 - 4);


    public static GaussianHarmonics<byte> Quantize(this GaussianHarmonics<float> harmonics)
    {
        GaussianHarmonics<byte> quantized = new();
        
        for (int i = 0; i < GaussianHarmonics<byte>.COEFFICIENT_COMPONENTS; i++)
        {
            if (i < 9)
                quantized[i] = QuantizeSH(harmonics[i], SH1_BUCKET_SIZE);
            else
                quantized[i] = QuantizeSH(harmonics[i], SHREST_BUCKET_SIZE);
        }

        return quantized;
    }


    public static GaussianHarmonics<float> Unquantize(this GaussianHarmonics<byte> harmonics)
    {
        GaussianHarmonics<float> unquantized = new();
        
        for (int i = 0; i < GaussianHarmonics<byte>.COEFFICIENT_COMPONENTS; i++)
            unquantized[i] = UnquantizeSH(harmonics[i]);

        return unquantized;
    }



    public static void Quantize(this Span<float> harmonics, Span<byte> quantized)
    {
        int i = harmonics.Length;
        while (i-- > 0)
        {
            if (i < 9)
                quantized[i] = QuantizeSH(harmonics[i], SH1_BUCKET_SIZE);
            else
                quantized[i] = QuantizeSH(harmonics[i], SHREST_BUCKET_SIZE);
        }
    }


    public static void Unquantize(this Span<byte> quantized, Span<float> harmonics)
    {   
        int i = quantized.Length;
        while(i-- > 0)
            harmonics[i] = UnquantizeSH(quantized[i]);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
        return Math.Min(Math.Max(min, value), max);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Clamp(in Vector3 value, in Vector3 min, in Vector3 max)
    {
        return Vector3.Min(Vector3.Max(min, value), max);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 Clamp(in Vector3 value, in float min, in float max)
    {
        return Clamp(value, new Vector3(min), new Vector3(max));
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ByteClamp(this float value) => (byte)Math.Min(Math.Max(0d, Math.Round(value)), 255d);
}
