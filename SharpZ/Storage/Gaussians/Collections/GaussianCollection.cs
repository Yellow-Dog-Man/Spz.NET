using System.Collections;

namespace SharPZ;

public abstract class GaussianCollection(int capacity, int shDim, GaussianFlags flags = 0, int fractionalBits = -1) : IReadOnlyList<Gaussian>
{
    public abstract Gaussian this[int index] { get; set; }

    public virtual bool Compressed { get; }
    public readonly int FractionalBits = fractionalBits;

    public int Count => capacity;
    public bool IsReadOnly => true;


    public int ShDim { get; } = shDim;
    public int ShDegree { get; } = SplatMathHelpers.DegreeForDim(shDim);
    public GaussianFlags Flags => flags;


    public bool Contains(Gaussian gaussian) => ContainsImpl(gaussian);
    protected abstract bool ContainsImpl(Gaussian Gaussian);


    public void CopyTo(Gaussian[] array, int arrayIndex) => CopyToImpl(array, arrayIndex);
    protected abstract void CopyToImpl(Gaussian[] array, int arrayIndex);


    IEnumerator<Gaussian> IEnumerable<Gaussian>.GetEnumerator() => GetTypedEnumeratorImpl();
    protected abstract IEnumerator<Gaussian> GetTypedEnumeratorImpl();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumeratorImpl();
    protected abstract IEnumerator GetEnumeratorImpl();



    public struct GaussianEnumerator(PackedGaussianCloud cloud) : IEnumerator<Gaussian>
    {
        private int curIndex = -1;
        readonly Gaussian IEnumerator<Gaussian>.Current
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
