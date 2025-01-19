using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;



public class GaussianCloud(int capacity, int shDim, bool antialiased = false) : GaussianCollection<Gaussian>(capacity, shDim, antialiased)
{
    public override bool Compressed => false;
    public override Gaussian this[int index]
    {
        get => gaussians[index];
        set => gaussians[index] = value;
    }


    protected readonly Gaussian[] gaussians = new Gaussian[capacity];


    protected override void CopyToImpl(Gaussian[] array, int arrayIndex) => gaussians.CopyTo(array, arrayIndex);
    protected override bool ContainsImpl(Gaussian gaussian) => gaussians.Contains(gaussian);
    protected override IEnumerator<Gaussian> GetTypedEnumeratorImpl() => (IEnumerator<Gaussian>)gaussians.GetEnumerator();
    protected override IEnumerator GetEnumeratorImpl() => gaussians.GetEnumerator();

}


public abstract class GaussianCollection<T>(int capacity, int shDim, bool antialiased = false) : IReadOnlyList<T>
    where T : unmanaged, IGaussian
{
    public abstract T this[int index] { get; set; }

    public virtual bool Compressed { get; }

    public int Count => capacity;
    public bool IsReadOnly => true;


    public int ShDegree { get; } = SplatSerializationHelper.DegreeForDim(shDim);
    public bool Antialiased => antialiased;


    public bool Contains(T gaussian) => ContainsImpl(gaussian);
    protected abstract bool ContainsImpl(T Gaussian);


    public void CopyTo(T[] array, int arrayIndex) => CopyToImpl(array, arrayIndex);
    protected abstract void CopyToImpl(T[] array, int arrayIndex);


    IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetTypedEnumeratorImpl();
    protected abstract IEnumerator<T> GetTypedEnumeratorImpl();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorImpl();
    protected abstract IEnumerator GetEnumeratorImpl();
}


public class PackedGaussianCloud(int capacity, int shDim, int fractionalBits = PackedGaussian.DEFAULT_FRACTIONAL_BITS, bool antialiased = false) : GaussianCollection<PackedGaussian>(capacity, shDim, antialiased)
{
    public override bool Compressed => true;
    public readonly int FractionalBits = fractionalBits;

    public override PackedGaussian this[int index]
    {
        get => new(
            FractionalBits,
            positions[index],
            scales[index],
            rotations[index],
            alphas[index],
            colors[index],
            sh[index]
        );

        set
        {
            positions[index] = value.PackedPosition;
            scales[index] = value.PackedScale;
            rotations[index] = value.PackedRotation;
            alphas[index] = value.PackedAlpha;
            colors[index] = value.PackedColor;
            sh[index] = value.PackedSh;
        }
    }


    internal readonly FixedVector3[] positions = new FixedVector3[capacity];
    internal readonly QuantizedScale[] scales = new QuantizedScale[capacity];
    internal readonly QuantizedQuat[] rotations = new QuantizedQuat[capacity];
    internal readonly QuantizedAlpha[] alphas = new QuantizedAlpha[capacity];
    internal readonly QuantizedColor[] colors = new QuantizedColor[capacity];
    internal readonly QuantizedHarmonics[] sh = new QuantizedHarmonics[capacity];


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