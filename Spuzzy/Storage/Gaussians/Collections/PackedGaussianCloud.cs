using System.Collections;
using Spuzzy.Enums;
using Spuzzy.Helpers;

namespace Spuzzy;

public class PackedGaussianCloud : GaussianCollection
{
    public const int DEFAULT_FRACTIONAL_BITS = 12;
    public override bool Compressed => true;

    public override Gaussian this[int index]
    {
        get => new(
            positions[index].ToVector3(FractionalBits),
            scales[index],
            rotations[index],
            alphas[index],
            colors[index],
            new GaussianHarmonics<byte>(sh.GetRowSpan(index)).Unquantize()
        );

        set
        {
            positions[index] = value.Position.ToFixed(FractionalBits);
            scales[index] = value.Scale;
            rotations[index] = value.Rotation;
            alphas[index] = value.Alpha;
            colors[index] = value.Color;
            
            var transposedRow = sh.GetRowSpan(index);

            GaussianHarmonics<byte> packed = value.Sh.Quantize();
            int i = transposedRow.Length;
            while (i-- > 0)
                transposedRow[i] = packed[i];
        }
    }


    internal readonly FixedVector3[] positions;
    internal readonly QuantizedScale[] scales;
    internal readonly QuantizedQuat[] rotations;
    internal readonly QuantizedAlpha[] alphas;
    internal readonly QuantizedColor[] colors;
    internal readonly MatrixView<byte> sh;

    public PackedGaussianCloud(int capacity, int shDim, int fractionalBits = DEFAULT_FRACTIONAL_BITS, GaussianFlags flags = 0) : base(capacity, shDim, flags, fractionalBits)
    {
        positions = new FixedVector3[capacity];
        scales = new QuantizedScale[capacity];
        rotations = new QuantizedQuat[capacity];
        alphas = new QuantizedAlpha[capacity];
        colors = new QuantizedColor[capacity];
        sh = new(shDim * 3, capacity);
    }

    protected override bool ContainsImpl(Gaussian Gaussian) => throw new NotImplementedException();
    protected override void CopyToImpl(Gaussian[] array, int arrayIndex) => throw new NotImplementedException();


    protected override IEnumerator<Gaussian> GetTypedEnumeratorImpl() => new GaussianEnumerator(this);
    protected override IEnumerator GetEnumeratorImpl() => new GaussianEnumerator(this);



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
