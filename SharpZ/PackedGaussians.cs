using System.Numerics;

namespace SharPZ;

public class PackedGaussians(
    int numPoints,
    int shDegree,
    bool antialiased,
    int fractionalBits = 12)
{
    public readonly int NumPoints = numPoints;
    public readonly int ShDegree = shDegree;
    public readonly int FractionalBits = fractionalBits;
    public readonly bool Antialiased = antialiased;


    public readonly byte[] Positions = new byte[numPoints * 3 * 3];
    public readonly byte[] Scales = new byte[numPoints * 3];
    public readonly byte[] Rotations = new byte[numPoints * 3];
    public readonly byte[] Alphas = new byte[numPoints];
    public readonly byte[] Colors = new byte[numPoints * 3];
    public readonly byte[] Sh = new byte[numPoints * SplatSerializationHelpers.DimForDegree(shDegree) * 3];
}


