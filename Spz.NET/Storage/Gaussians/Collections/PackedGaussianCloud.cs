using System.Collections;
using Spz.NET.Enums;
using Spz.NET.Helpers;

namespace Spz.NET;

/// <summary>
/// Represents a compressed collection of gaussians representing a gaussian splat.
/// </summary>
public class PackedGaussianCloud : GaussianCollection
{
    public const int DEFAULT_FRACTIONAL_BITS = 12;

    /// <inheritdoc/>
    public override bool Compressed => true;

    /// <inheritdoc/>
    public override Gaussian this[int index]
    {
        get => new(
            positions[index].ToVector3(FractionalBits),
            scales[index],
            rotations[index],
            alphas[index],
            colors[index],
            GaussianHarmonics<byte>.From(sh.GetRowSpan(index)).Unquantize()
        );

        set
        {
            positions[index] = value.Position.ToFixed(FractionalBits);
            scales[index] = value.Scale;
            rotations[index] = value.Rotation;
            alphas[index] = value.Alpha;
            colors[index] = value.Color;
            
            var row = sh.GetRowSpan(index);

            GaussianHarmonics<byte> packed = value.Sh.Quantize();
            packed.To(row);
        }
    }


    internal readonly FixedVector3[] positions;
    internal readonly QuantizedScale[] scales;
    internal readonly QuantizedQuat[] rotations;
    internal readonly QuantizedAlpha[] alphas;
    internal readonly QuantizedColor[] colors;
    internal readonly MatrixView<byte> sh;


    /// <summary>
    /// Creates a cloud of gaussians with the specified parameters.
    /// </summary>
    /// <param name="capacity">The capacity of the cloud.</param>
    /// <param name="shDim">The dimensions of each gaussian's spherical harmonics.</param>
    /// <param name="fractionalBits">The number of bits dedicated to the fractional portion of each gaussian's position.</param>
    /// <param name="flags">The collection flags.</param>
    public PackedGaussianCloud(int capacity, int shDim, int fractionalBits = DEFAULT_FRACTIONAL_BITS, GaussianFlags flags = 0) : base(capacity, shDim, flags, fractionalBits)
    {
        positions = new FixedVector3[capacity];
        scales = new QuantizedScale[capacity];
        rotations = new QuantizedQuat[capacity];
        alphas = new QuantizedAlpha[capacity];
        colors = new QuantizedColor[capacity];
        sh = new(shDim * 3, capacity);
    }



    /// <summary>
    /// Decompresses this collection into an unpacked gaussian cloud.
    /// </summary>
    /// <returns>An decompressed cloud of gaussians.</returns>
    public GaussianCloud Unpack()
    {
        GaussianCloud unpacked = new(Count, SplatMathHelpers.DimForDegree(ShDegree), Flags);

        for (int i = 0; i < Count; i++)
        {
            unpacked.positions[i] = positions[i].ToVector3(FractionalBits);
            unpacked.alphas[i] = alphas[i];
            unpacked.colors[i] = colors[i];
            unpacked.scales[i] = scales[i];
            unpacked.rotations[i] = rotations[i];


            // if (unpacked.positions[i].Length() > 2047f)
            // {
            //     Vector3 v3 = unpacked.positions[i];
            //     FixedVector3 f3 = packed.positions[i];
            //     throw new IndexOutOfRangeException($"WHAT THE FFFF: {unpacked.positions[i]}");
            // }


            var harmonic = unpacked.sh.GetRowSpan(i);
            var packedHarmonic = sh.GetRowSpan(i);

            packedHarmonic.Unquantize(harmonic);
        }

        return unpacked;
    }
}
