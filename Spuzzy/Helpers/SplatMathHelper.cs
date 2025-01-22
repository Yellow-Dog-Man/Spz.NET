using System.Numerics;
using System.Runtime.CompilerServices;

namespace Spuzzy.Helpers;

public static class SplatMathHelpers
{
    const int SH1_BUCKET_SIZE = 1 << (8 - 5);
    const int SHREST_BUCKET_SIZE = 1 << (8 - 4);


    /// <summary>
    /// Quantizes a gaussian's spherical harmonics for improved compression performance.
    /// <para>
    /// 5 bits are used for the first 3 coefficients (9 components), and 4 bits are used for the rest.
    /// </para>
    /// </summary>
    /// <param name="harmonics">The harmonics to quantize.</param>
    /// <returns>Quantized harmonics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GaussianHarmonics<byte> Quantize(this in GaussianHarmonics<float> harmonics)
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


    /// <summary>
    /// Unquantizes a gaussian's spherical harmonics.
    /// </summary>
    /// <para>
    /// 5 bits are used for the first 3 coefficients (9 components), and 4 bits are used for the rest.
    /// </para>
    /// <param name="harmonics">The harmonics to unquantize.</param>
    /// <returns>Unquantized harmonics.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GaussianHarmonics<float> Unquantize(this in GaussianHarmonics<byte> harmonics)
    {
        GaussianHarmonics<float> unquantized = new();
        
        for (int i = 0; i < GaussianHarmonics<byte>.COEFFICIENT_COMPONENTS; i++)
            unquantized[i] = UnquantizeSH(harmonics[i]);

        return unquantized;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


    /// <summary>
    /// Quantizes a single component of a spherical harmonic's coefficients to a specified bucket size.
    /// </summary>
    /// <param name="x">The component value.</param>
    /// <param name="bucketSize">The size of the bucket to quantize to.</param>
    /// <returns>A quantized representation of the component.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte QuantizeSH(float x, int bucketSize)
    {
        int q = (int)(Math.Round(x * 128f) + 128f);

        q = (q + bucketSize / 2) / bucketSize * bucketSize;
        return (byte)Clamp(q, 0, 255);
    }


    /// <summary>
    /// Unquantizes a single component of a spherical harmonic's coefficients.
    /// </summary>
    /// <param name="x">The quantized value.</param>
    /// <returns>An unquantized representation of the component.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float UnquantizeSH(byte x) => (x - 128f) / 128f;



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sigmoid(float x) => 1f / (1f + (float)Math.Exp(-x));

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InvSigmoid(float x) => (float)Math.Log(x / (1f - x));


    /// <summary>
    /// Gets the number of coefficients for a given degree of spherical harmonics.
    /// </summary>
    /// <param name="degree">The degree of spherical harmonics.</param>
    /// <returns>The dimensions, or number of coefficients for the given degree.</returns>
    /// <exception cref="NotImplementedException">Thrown when a non-supported degree of spherical harmonic is requested.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DimForDegree(int degree)
    {
        return degree switch
        {
            0 => 0,
            1 => 3,
            2 => 8,
            3 => 15,
            _ => throw new NotImplementedException($"Unsupported SH degree: {degree}")
        };
    }


    /// <summary>
    /// Gets the degree of spherical harmonics for a given number of coefficients.
    /// </summary>
    /// <param name="dim">The coefficients in the spherical harmonic.</param>
    /// <returns>The corresponding degree.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int DegreeForDim(int dim)
    {
        if (dim < 3)
            return 0;

        if (dim < 8)
            return 1;

        if (dim < 15)
            return 2;

        return 3;
    }
}
