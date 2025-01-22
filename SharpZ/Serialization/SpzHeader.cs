namespace SharPZ;

public readonly struct SpzHeader(uint magic, uint version, uint points, byte shDegree, byte fractionalBits, GaussianFlags flags = 0, byte reserved = 0)
{
    public const uint MAGIC = 0x5053474e;
    public const uint VERSION = 2;


    public readonly uint Magic = magic;
    public readonly uint Version = version;
    public readonly uint NumPoints = points;
    public readonly byte ShDegree = shDegree;
    public readonly byte FractionalBits = fractionalBits;
    public readonly GaussianFlags Flags = flags;
    public readonly byte Reserved = reserved;



    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(Magic);
        writer.Write(Version);
        writer.Write(NumPoints);
        writer.Write(ShDegree);
        writer.Write(FractionalBits);
        writer.Write((byte)Flags);
        writer.Write(Reserved);
    }



    public static SpzHeader ReadFrom(BinaryReader reader)
    {
        SpzHeader readHeader = new(
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadByte(),
            reader.ReadByte(),
            (GaussianFlags)reader.ReadByte(),
            reader.ReadByte()
        );

        if (readHeader.Magic != MAGIC)
            throw new SplatFormatException("SPZ header not found.");
        
        if (readHeader.Version != VERSION)
            throw new SplatFormatException($"SPZ version not supported: {readHeader.Version}");
        
        if (readHeader.NumPoints > SplatSerializer.SPZ_MAX_POINTS)
            throw new SplatFormatException($"SPZ has too many points: {readHeader.NumPoints}");
        
        if (readHeader.ShDegree > 3)
            throw new SplatFormatException($"SPZ has unsupported spherical harmonics degree: {readHeader.ShDegree}");
        
        
        return readHeader;
    }
}


