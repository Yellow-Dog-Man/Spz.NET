using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
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
    public static GaussianCloud FromSpz(Stream stream)
    {
        using GZipStream decompressor = new(stream, CompressionMode.Decompress);
        using BinaryReader reader = new(decompressor);


        SpzHeader header = SpzHeader.ReadFrom(reader);
        int count = (int)header.NumPoints;
        int shDim = SplatMathHelpers.DimForDegree(header.ShDegree);
        int shCount = shDim * 3;
        GaussianCloud cloud = new(count, shDim, header.Flags);


        using QuickArray<FixedVector3> positions = new(count);
        using QuickArray<QuantizedAlpha> alphas = new(count);
        using QuickArray<QuantizedColor> colors = new(count);
        using QuickArray<QuantizedScale> scales = new(count);
        using QuickArray<QuantizedQuat>  rotations = new(count);
        using QuickArray<byte> harmonics = new(count * shDim * 3);

        Span<FixedVector3> posSpan = positions.Span;
        Span<QuantizedAlpha> alphaSpan = alphas.Span;
        Span<QuantizedColor> colorSpan = colors.Span;
        Span<QuantizedScale> scaleSpan = scales.Span;
        Span<QuantizedQuat> rotSpan = rotations.Span;
        Span<byte> harmonicSpan = harmonics.Span;

        void ReadAll(Span<byte> buffer)
        {
            var toRead = buffer.Length;

            while (toRead > 0)
            {
                int read = reader.Read(buffer.Slice(buffer.Length - toRead));

                if (read == 0)
                    throw new EndOfStreamException("Unexpected end of stream");

                toRead -= read;
            }
        }

        ReadAll(positions.Bytes);
        ReadAll(alphas.Bytes);
        ReadAll(colors.Bytes);
        ReadAll(scales.Bytes);
        ReadAll(rotations.Bytes);
        ReadAll(harmonics.Bytes);

        int i = count;
        while (i-- > 0)
        {
            int offset = i * shCount;
            harmonicSpan.Unquantize(out GaussianHarmonics gaussianHarmonics, offset, shCount);

            cloud[i] = new(
                posSpan[i].ToVector3(12),
                scaleSpan[i],
                rotSpan[i],
                alphaSpan[i],
                colorSpan[i],
                gaussianHarmonics
            );
        }



        return cloud;
    }



    /// <summary>
    /// Deserializes a gaussian splat from a file path pointing to an SPZ file.
    /// </summary>
    /// <param name="filePath">The path to the SPZ file.</param>
    /// <returns>A compressed cloud of gaussians containing the deserialized data.</returns>
    public static GaussianCloud FromSpz(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);

        return FromSpz(stream);
    }
    

    /// <summary>
    /// Serializes this gaussian splat to a stream and encodes it in the SPZ file format.
    /// </summary>
    /// <param name="gaussians">The compressed cloud of gaussians to serialize.</param>
    /// <param name="stream">The stream to serialize this compressed gaussian cloud to.</param>
    public static void ToSpz(this GaussianCloud gaussians, Stream stream)
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

        int count = gaussians.Count;
        int shCount = gaussians.ShDim * 3;

        using QuickArray<FixedVector3> positions = new(count);
        using QuickArray<QuantizedAlpha> alphas = new(count);
        using QuickArray<QuantizedColor> colors = new(count);
        using QuickArray<QuantizedScale> scales = new(count);
        using QuickArray<QuantizedQuat>  rotations = new(count);
        using QuickArray<byte> harmonics = new(count * shCount);


        Span<FixedVector3> posSpan = positions.Span;
        Span<QuantizedAlpha> alphaSpan = alphas.Span;
        Span<QuantizedColor> colorSpan = colors.Span;
        Span<QuantizedScale> scaleSpan = scales.Span;
        Span<QuantizedQuat> rotSpan = rotations.Span;
        Span<byte> harmonicSpan = harmonics.Span;


        int i = count;
        while (i-- > 0)
        {
            int offset = i * shCount;
            Gaussian cur = gaussians[i];
            posSpan[i] = cur.Position.ToFixed(12);
            alphaSpan[i] = cur.Alpha;
            colorSpan[i] = cur.Color;
            scaleSpan[i] = cur.Scale;
            rotSpan[i] = cur.Rotation;
            cur.Sh.Quantize(harmonicSpan, offset, shCount);
        }

        writer.Write(positions.Bytes);
        writer.Write(alphas.Bytes);
        writer.Write(colors.Bytes);
        writer.Write(scales.Bytes);
        writer.Write(rotations.Bytes);
        writer.Write(harmonics.Bytes);
    }


    /// <summary>
    /// Serializes this gaussian cloud to file at the specified path in the SPZ file format.
    /// </summary>
    /// <param name="gaussians">The compressed cloud of gaussians to serialize.</param>
    /// <param name="filePath">The path to the file where this gaussian cloud will be written to.</param>
    public static void ToSpz(this GaussianCloud gaussians, string filePath)
    {
        using FileStream stream = File.OpenWrite(filePath);

        ToSpz(gaussians, stream);
    }
}



public unsafe struct QuickArray<T> : IDisposable
    where T : unmanaged
{
    public readonly int Count { get; }
    
    public readonly Span<T> Span => disposed ? throw new ObjectDisposedException(nameof(QuickArray<T>)) : new(memory, Count);
    public readonly Span<byte> Bytes => MemoryMarshal.Cast<T, byte>(Span);

    private readonly void* memory;
    private bool disposed;



    public QuickArray(int count)
    {
        Count = count;

        memory = (void*)Marshal.AllocHGlobal(count * Unsafe.SizeOf<T>());
    }


    public void Dispose()
    {
        disposed = true;
        Marshal.FreeHGlobal((IntPtr)memory);
    }

}