namespace SharPZ;

public struct PackedGaussiansHeader(uint magic, uint version, uint points, byte shDegree, byte fractionalBits, byte flags, byte reserved)
{
    public const uint MAGIC = 0x5053474e;
    public const uint VERSION = 2;


    public readonly uint Magic = magic;
    public readonly uint Version = version;
    public readonly uint NumPoints = points;
    public readonly byte ShDegree = shDegree;
    public readonly byte FractionalBits = fractionalBits;
    public readonly byte Flags = flags;
    public readonly byte Reserved = reserved;
}


