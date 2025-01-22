using System.Collections;
using Spz.NET.Enums;
using Spz.NET.Helpers;

namespace Spz.NET;


/// <summary>
/// A collection of gaussians representing a gaussian splat.
/// </summary>
public abstract class GaussianCollection(int capacity, int shDim, GaussianFlags flags = 0, int fractionalBits = -1) : IReadOnlyList<Gaussian>
{
    /// <summary>
    /// Retrieves a gaussian from the specified index in the collection.
    /// </summary>
    /// <param name="index">Index of the gaussian.</param>
    /// <returns>The retrieved gaussian.</returns>
    public abstract Gaussian this[int index] { get; set; }

    /// <summary>
    /// Whether this collection is compressed.
    /// </summary>
    public virtual bool Compressed { get; }

    /// <summary>
    /// The number of bits dedicated to representing the fractional portion of each gaussian's position.
    /// </summary>
    public readonly int FractionalBits = fractionalBits;

    /// <summary>
    /// The number of gaussians in this collection.
    /// </summary>
    public int Count => capacity;


    public bool IsReadOnly => true;


    /// <summary>
    /// The dimensions of the spherical harmonics for each gaussian.
    /// </summary>
    public readonly int ShDim = shDim;

    /// <summary>
    /// The degree of the spherical harmonics for each gaussian.
    /// </summary>
    public readonly int ShDegree = SplatMathHelpers.DegreeForDim(shDim);

    /// <summary>
    /// Collection flags.
    /// </summary>
    public GaussianFlags Flags => flags;


    public bool Contains(in Gaussian gaussian)
    {
        int i = Count;

        while (i-- > 0)
        {
            if (this[i] == gaussian)
                return true;
        }

        return false;
    }


    public void CopyTo(Gaussian[] array, int arrayIndex)
    {
        Span<Gaussian> arraySpan = array.AsSpan()[arrayIndex..];

        if (arraySpan.Length < Count)
            throw new ArgumentOutOfRangeException(nameof(array), $"Length of destination array after the specified index is too short.");

        int i = Count;
        while (i-- > 0)
            arraySpan[i] = this[i];
    }


    IEnumerator<Gaussian> IEnumerable<Gaussian>.GetEnumerator() => new GaussianEnumerator(this);

    IEnumerator IEnumerable.GetEnumerator() => new GaussianEnumerator(this);



    public struct GaussianEnumerator(GaussianCollection collection) : IEnumerator<Gaussian>
    {
        private int curIndex = -1;
        readonly Gaussian IEnumerator<Gaussian>.Current
        {
            get
            {
                try
                {
                    return collection[curIndex];
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
                    return collection[curIndex];
                }
                catch (IndexOutOfRangeException)
                {
                    return null;
                }
            }
        }

        public bool MoveNext() => ++curIndex < collection.Count;
        public void Reset() => curIndex = -1;

        public readonly void Dispose() { }
    }
}
