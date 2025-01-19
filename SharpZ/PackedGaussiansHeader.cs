namespace SharPZ;

public struct PackedGaussiansHeader(uint points, byte shDegree, byte fractionalBits, byte flags)
{
    public const uint MAGIC = 0x5053474e;
    public const uint VERSION = 2;


    public readonly uint Magic = MAGIC;
    public readonly uint Version = VERSION;
    public readonly uint NumPoints;
    public readonly byte ShDegree;
    public readonly byte FractionalBits;
    public readonly byte Flags;
    public readonly byte Reserved;
}


