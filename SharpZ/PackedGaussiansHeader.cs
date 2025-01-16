namespace SharPZ;

public struct PackedGaussiansHeader(uint points, byte shDegree, byte fractionalBits, byte flags)
{
    public readonly uint Magic = 0x5053474e;
    public readonly uint Version = 2;
    public readonly uint NumPoints;
    public readonly byte ShDegree;
    public readonly byte FractionalBits;
    public readonly byte Flags;
    public readonly byte Reserved;
}


