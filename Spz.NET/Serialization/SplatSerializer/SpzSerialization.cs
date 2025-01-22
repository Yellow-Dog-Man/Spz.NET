using System.Buffers;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Spz.NET.Helpers;
using Spz.NET.Structs;

namespace Spz.NET.Serialization;

public static partial class SplatSerializer
{
    public const int SPZ_MAX_POINTS = 10000000;
    

    /// <summary>
    /// Deserializes a gaussian splat from a stream providing an SPZ file.
    /// </summary>
    /// <param name="stream">The stream to read the SPZ file from.</param>
    /// <returns>A compressed cloud of gaussians containing the deserialized data.</returns>
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



    /// <summary>
    /// Deserializes a gaussian splat from a file path pointing to an SPZ file.
    /// </summary>
    /// <param name="filePath">The path to the SPZ file.</param>
    /// <returns>A compressed cloud of gaussians containing the deserialized data.</returns>
    public static PackedGaussianCloud FromSpz(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);

        return FromSpz(stream);
    }
    

    /// <summary>
    /// Serializes this gaussian splat to a stream and encodes it in the SPZ file format.
    /// </summary>
    /// <param name="gaussians">The compressed cloud of gaussians to serialize.</param>
    /// <param name="stream">The stream to serialize this compressed gaussian cloud to.</param>
    public static void ToSpz(this PackedGaussianCloud gaussians, Stream stream)
    {
        SpzHeader header = new(
            SpzHeader.MAGIC,
            SpzHeader.VERSION,
            (uint)gaussians.Count,
            (byte)gaussians.ShDegree,
            (byte)gaussians.FractionalBits,
            gaussians.Flags
        );


        using GZipStream zipper = new(stream, CompressionLevel.Optimal);
        using BinaryWriter writer = new(zipper);

        header.WriteTo(writer);

        Span<byte> posBytes = MemoryMarshal.Cast<FixedVector3, byte>(gaussians.positions);
        writer.Write(posBytes);

        Span<byte> alphaBytes = MemoryMarshal.Cast<QuantizedAlpha, byte>(gaussians.alphas);
        writer.Write(alphaBytes);
        
        Span<byte> colorBytes = MemoryMarshal.Cast<QuantizedColor, byte>(gaussians.colors);
        writer.Write(colorBytes);

        Span<byte> scaleBytes = MemoryMarshal.Cast<QuantizedScale, byte>(gaussians.scales);
        writer.Write(scaleBytes);

        Span<byte> rotationBytes = MemoryMarshal.Cast<QuantizedQuat, byte>(gaussians.rotations);
        writer.Write(rotationBytes);

        writer.Write(gaussians.sh.Span);
    }

    /// <summary>
    /// Serializes this gaussian cloud to file at the specified path in the SPZ file format.
    /// </summary>
    /// <param name="gaussians">The compressed cloud of gaussians to serialize.</param>
    /// <param name="filePath">The path to the file where this gaussian cloud will be written to.</param>
    public static void ToSpz(this PackedGaussianCloud gaussians, string filePath)
    {
        using FileStream stream = File.OpenWrite(filePath);

        ToSpz(gaussians, stream);
    }
}