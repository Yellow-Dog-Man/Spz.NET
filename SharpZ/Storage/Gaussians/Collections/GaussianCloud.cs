using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;



public class GaussianCloud : GaussianCollection
{
    public override bool Compressed => false;
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
            

            Span<GaussianHarmonics<float>> harmonic = [ value.Sh ];
            Span<float> harmonicCoeffs = MemoryMarshal.Cast<GaussianHarmonics<float>, float>(harmonic);

            var dest = sh.GetRowSpan(index);
            harmonicCoeffs.CopyTo(dest);
        }
    }


    internal readonly Vector3[] positions;
    internal readonly Vector3[] scales;
    internal readonly Quaternion[] rotations;
    internal readonly float[] alphas;
    internal readonly Vector3[] colors;
    internal readonly MatrixView<float> sh;

    public GaussianCloud(int capacity, int shDim, GaussianFlags flags = 0) : base(capacity, shDim, flags)
    {
        positions = new Vector3[capacity];
        scales = new Vector3[capacity];
        rotations = new Quaternion[capacity];
        alphas = new float[capacity];
        colors = new Vector3[capacity];
        sh = new(shDim * 3, capacity);
    }

    // protected readonly Gaussian[] gaussians = new Gaussian[capacity];


    protected override void CopyToImpl(Gaussian[] array, int arrayIndex) => throw new NotImplementedException();
    protected override bool ContainsImpl(Gaussian gaussian) => throw new NotImplementedException();
    protected override IEnumerator<Gaussian> GetTypedEnumeratorImpl() => throw new NotImplementedException();
    protected override IEnumerator GetEnumeratorImpl() => throw new NotImplementedException();



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
