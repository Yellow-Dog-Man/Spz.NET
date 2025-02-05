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
/// <remarks>
/// Creates a cloud of gaussians with the specified parameters.
/// </remarks>
/// <param name="capacity">The capacity of the cloud.</param>
/// <param name="shDim">The dimensions of each gaussian's spherical harmonics.</param>
/// <param name="flags">The collection flags.</param>
public class GaussianCloud(int capacity, int shDim, GaussianFlags flags = 0) : GaussianCollection(capacity, shDim, flags)
{
    /// <inheritdoc/>
    public override bool Compressed => false;

    /// <inheritdoc/>
    public override Gaussian this[int index]
    {
        get => gaussians[index];
        set => gaussians[index ] = value;
    }


    internal readonly Gaussian[] gaussians = new Gaussian[capacity];
}
