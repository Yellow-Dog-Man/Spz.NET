using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;



public class GaussianCloud : GaussianCollection<Gaussian>
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

}


public abstract class GaussianCollection<T>(int capacity, int shDim, GaussianFlags flags = 0) : IReadOnlyList<T>
    where T : unmanaged, IGaussian
{
    public abstract T this[int index] { get; set; }

    public virtual bool Compressed { get; }

    public int Count => capacity;
    public bool IsReadOnly => true;


    public int ShDim { get; } = shDim;
    public int ShDegree { get; } = SplatSerializationHelpers.DegreeForDim(shDim);
    public GaussianFlags Flags => flags;


    public bool Contains(T gaussian) => ContainsImpl(gaussian);
    protected abstract bool ContainsImpl(T Gaussian);


    public void CopyTo(T[] array, int arrayIndex) => CopyToImpl(array, arrayIndex);
    protected abstract void CopyToImpl(T[] array, int arrayIndex);


    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetTypedEnumeratorImpl();
    protected abstract IEnumerator<T> GetTypedEnumeratorImpl();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorImpl();
    protected abstract IEnumerator GetEnumeratorImpl();
}


public class PackedGaussianCloud : GaussianCollection<PackedGaussian>
{
    public override bool Compressed => true;
    public readonly int FractionalBits;

    public override PackedGaussian this[int index]
    {
        get => new(
            FractionalBits,
            positions[index],
            scales[index],
            rotations[index],
            alphas[index],
            colors[index],
            new(sh.GetRow(index))
        );

        set
        {
            positions[index] = value.PackedPosition;
            scales[index] = value.PackedScale;
            rotations[index] = value.PackedRotation;
            alphas[index] = value.PackedAlpha;
            colors[index] = value.PackedColor;
            
            var transposedRow = sh.GetRow(index);

            for (int i = 0; i < transposedRow.Length; i++)
            {
                transposedRow[i] = value.PackedSh[i];
            }
        }
    }


    internal readonly FixedVector3[] positions;
    internal readonly QuantizedScale[] scales;
    internal readonly QuantizedQuat[] rotations;
    internal readonly QuantizedAlpha[] alphas;
    internal readonly QuantizedColor[] colors;
    internal readonly MatrixView<byte> sh;

    public PackedGaussianCloud(int capacity, int shDim, int fractionalBits = PackedGaussian.DEFAULT_FRACTIONAL_BITS, GaussianFlags flags = 0) : base(capacity, shDim, flags)
    {
        FractionalBits = fractionalBits;
        positions = new FixedVector3[capacity];
        scales = new QuantizedScale[capacity];
        rotations = new QuantizedQuat[capacity];
        alphas = new QuantizedAlpha[capacity];
        colors = new QuantizedColor[capacity];
        sh = new(shDim * 3, capacity);
    }

    protected override bool ContainsImpl(PackedGaussian Gaussian) => throw new NotImplementedException();
    protected override void CopyToImpl(PackedGaussian[] array, int arrayIndex) => throw new NotImplementedException();


    protected override IEnumerator<PackedGaussian> GetTypedEnumeratorImpl() => new PackedGaussianEnumerator(this);
    protected override IEnumerator GetEnumeratorImpl() => new PackedGaussianEnumerator(this);






    public struct PackedGaussianEnumerator(PackedGaussianCloud cloud) : IEnumerator<PackedGaussian>
    {
        private int curIndex = -1;
        readonly PackedGaussian IEnumerator<PackedGaussian>.Current
        {
            get
            {
                try
                {
                    return cloud[curIndex];
                }
                catch (IndexOutOfRangeException)
                {
                    return default;
                }
            }
        }


        readonly object? IEnumerator.Current
        {
            get
            {
                try
                {
                    return cloud[curIndex];
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
            }
        }

        public bool MoveNext() => ++curIndex < cloud.Count;
        public void Reset() => curIndex = -1;

        public readonly void Dispose() { }
    }
}


[Flags]
public enum GaussianFlags : byte
{
    Antialiased = 1
}