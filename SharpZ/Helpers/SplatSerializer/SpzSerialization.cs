using System.Buffers;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;

public static partial class SplatSerializer
{
    public const int SPZ_MAX_POINTS = 10000000;
    
    public static PackedGaussianCloud FromSpz(Stream stream)
    {
        using GZipStream decompressor = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(decompressor);


        SpzHeader header = SpzHeader.ReadFrom(reader);
        int numPoints = (int)header.NumPoints;
        int shDim = SplatMathHelpers.DimForDegree(header.ShDegree);
        PackedGaussianCloud cloud = new(numPoints, shDim, header.FractionalBits, header.Flags);


        // Chunk of the biggest size of the largest data type.

        Span<byte> cloudPosBytes = MemoryMarshal.Cast<FixedVector3, byte>(cloud.positions);
        reader.Read(cloudPosBytes);
        
    
        Span<byte> cloudAlphaBytes = MemoryMarshal.Cast<QuantizedAlpha, byte>(cloud.alphas);
        reader.Read(cloudAlphaBytes);


        Span<byte> cloudColorBytes = MemoryMarshal.Cast<QuantizedColor, byte>(cloud.colors);
        reader.Read(cloudColorBytes);
        

        Span<byte> cloudScaleBytes = MemoryMarshal.Cast<QuantizedScale, byte>(cloud.scales);
        reader.Read(cloudScaleBytes);


        Span<byte> cloudRotationBytes = MemoryMarshal.Cast<QuantizedQuat, byte>(cloud.rotations);
        reader.Read(cloudRotationBytes);

        reader.Read(cloud.sh.Span);

        return cloud;
    }



    public static PackedGaussianCloud FromSpz(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);

        return FromSpz(stream);
    }
    


    public static void ToSpz(this PackedGaussianCloud cloud, Stream stream)
    {
        SpzHeader header = new(
            SpzHeader.MAGIC,
            SpzHeader.VERSION,
            (uint)cloud.Count,
            (byte)cloud.ShDegree,
            (byte)cloud.FractionalBits,
            cloud.Flags
        );


        using GZipStream zipper = new(stream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(zipper);

        header.WriteTo(writer);

        Span<byte> posBytes = MemoryMarshal.Cast<FixedVector3, byte>(cloud.positions);
        writer.Write(posBytes);

        Span<byte> alphaBytes = MemoryMarshal.Cast<QuantizedAlpha, byte>(cloud.alphas);
        writer.Write(alphaBytes);
        
        Span<byte> colorBytes = MemoryMarshal.Cast<QuantizedColor, byte>(cloud.colors);
        writer.Write(colorBytes);

        Span<byte> scaleBytes = MemoryMarshal.Cast<QuantizedScale, byte>(cloud.scales);
        writer.Write(scaleBytes);

        Span<byte> rotationBytes = MemoryMarshal.Cast<QuantizedQuat, byte>(cloud.rotations);
        writer.Write(rotationBytes);

        writer.Write(cloud.sh.Span);
    }

    public static void ToSpz(this PackedGaussianCloud cloud, string filePath)
    {
        using FileStream stream = File.OpenWrite(filePath);

        ToSpz(cloud, stream);
    }
}