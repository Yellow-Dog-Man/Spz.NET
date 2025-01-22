using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spz.NET.Enums;
using Spz.NET.Helpers;

namespace Spz.NET;


/// <summary>
/// A collection of uncompressed gaussians representing a gaussian splat.
/// </summary>
public class GaussianCloud : GaussianCollection
{
    /// <inheritdoc/>
    public override bool Compressed => false;

    /// <inheritdoc/>
    public override Gaussian this[int index]
    {
        get
        {
            Span<GaussianHarmonics<float>> harmonic = stackalloc GaussianHarmonics<float>[1];
            Span<float> harmonicCoeffs = MemoryMarshal.Cast<GaussianHarmonics<float>, float>(harmonic);

            var row = sh.GetRowSpan(index)[..(ShDim * 3)];
            row.CopyTo(harmonicCoeffs);

            return new(
                positions[index],
                scales[index],
                rotations[index],
                alphas[index],
                colors[index],
                harmonic[0]);
        }
        set
        {
            positions[index] = value.Position;
            scales[index] = value.Scale;
            rotations[index] = value.Rotation;
            alphas[index] = value.Alpha;
            colors[index] = value.Color;
            

            var row = sh.GetRowSpan(index);
            value.Sh.To(row);
        }
    }


    internal readonly Vector3[] positions;
    internal readonly Vector3[] scales;
    internal readonly Quaternion[] rotations;
    internal readonly float[] alphas;
    internal readonly Vector3[] colors;
    internal readonly MatrixView<float> sh;

    /// <summary>
    /// Creates a cloud of gaussians with the specified parameters.
    /// </summary>
    /// <param name="capacity">The capacity of the cloud.</param>
    /// <param name="shDim">The dimensions of each gaussian's spherical harmonics.</param>
    /// <param name="flags">The collection flags.</param>
    public GaussianCloud(int capacity, int shDim, GaussianFlags flags = 0) : base(capacity, shDim, flags)
    {
        positions = new Vector3[capacity];
        scales = new Vector3[capacity];
        rotations = new Quaternion[capacity];
        alphas = new float[capacity];
        colors = new Vector3[capacity];
        sh = new(shDim * 3, capacity);
    }


    /// <summary>
    /// Lossily compresses this collection into a more compact format.
    /// </summary>
    /// <param name="fractionalBits">The number of bits to use for the fractional portion of each gaussian's position.</param>
    /// <returns>A compressed cloud of gaussians.</returns>
    public PackedGaussianCloud Pack(int fractionalBits = PackedGaussianCloud.DEFAULT_FRACTIONAL_BITS)
    {
        int shDim = SplatMathHelpers.DimForDegree(ShDegree);
        PackedGaussianCloud packed = new(Count, shDim, fractionalBits, Flags);

        for (int i = 0; i < Count; i++)
        {
            packed.positions[i] = positions[i].ToFixed(fractionalBits);
            packed.alphas[i] = alphas[i];
            packed.colors[i] = colors[i];
            packed.scales[i] = scales[i];
            packed.rotations[i] = rotations[i];


            var harmonic = sh.GetRowSpan(i);
            var packedHarmonic = packed.sh.GetRowSpan(i);

            harmonic.Quantize(packedHarmonic);
        }

        return packed;
    }

}
