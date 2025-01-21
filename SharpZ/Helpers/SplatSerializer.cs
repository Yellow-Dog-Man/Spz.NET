using System.Buffers;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;

public static class SplatSerializer
{
    const string END_HEADER = "end_header";
    const string FLOAT_PROPERTY_MARKER = "property float ";



    /// <summary>
    /// Deserializes a gaussian cloud from a stream providing a PLY file.
    /// </summary>
    /// <param name="stream">The stream to read the PLY file from.</param>
    /// <returns>A cloud of gaussians containing the deserialized data.</returns>
    /// <exception cref="SplatFormatException">Thrown when decoding the gaussian splat fails.</exception>
    public static GaussianCloud FromPly(Stream stream)
    {
        // Create a binary reader to aid in reading the stream
        using BinaryReader reader = new(stream);

        // Validate the PLY file and return the number of points it has in it.
        int numPoints = SplatSerializationHelpers.ValidatePlySplat(reader);

        #region Property decoding

        // Lookup of field -> index. Each gaussian is a collection of fields.
        Dictionary<string, int> fields = [];

        // Helper function to get the index of each field within the gaussian. Throws if a field is missing.
        int index(string key)
        {
            if (!fields.TryGetValue(key, out int value))
                throw new SplatFormatException($"Field was missing: {key}");

            return value;
        }


        // Read the fields of each gaussian and add their indicies to the dictionary.
        for (int i = 0; /*End condition depends on file contents*/ ; i++)
        {
            string curLine = reader.ReadLine();
            if (curLine == END_HEADER)
                break;

            if (!curLine.StartsWith(FLOAT_PROPERTY_MARKER))
                throw new SplatFormatException($"Unsupported property data type: {curLine}");


            string name = curLine[FLOAT_PROPERTY_MARKER.Length..];
            fields.Add(name, i);
        }
        int fieldCount = fields.Count;


        // Get the field indicies of the spherical harmonics.
        Span<int> shIdx = stackalloc int[45];
        for (int i = 0; i < 45; i++)
        {
            try
            {
                shIdx[i] = index($"f_rest_{i}");
            }
            catch (SplatFormatException)
            {
                shIdx = shIdx[..i];
                break;
            }
        }
        int shDim = shIdx.Length / 3;

        #endregion



        #region Splat decoding

        // Make a temporary array to read the file in chunks. Each chunk will be a gaussian.
        Span<float> chunk = stackalloc float[fieldCount];
        Span<byte> chunkBytes = MemoryMarshal.Cast<float, byte>(chunk);

        // Create a gaussian cloud to store gaussians in.
        GaussianCloud cloud = new(numPoints, shDim, false);

        // Create a reader/writer with the indicies of each gaussian's properties within the chunk.
        SplatChunkReaderWriter chunkReader = new(
            chunk,
            [ index("x"), index("y"), index("z") ],
            [ index("scale_0"), index("scale_1"), index("scale_2") ],
            [ index("rot_0"), index("rot_1"), index("rot_2"), index("rot_3") ],
            index("opacity"),
            [ index("f_dc_0"), index("f_dc_1"), index("f_dc_2") ],
            shIdx,
            shDim
        );


        // Read a chunk and then decode a gaussian from that chunk.
        for (int i = 0; i < numPoints; i++)
        {
            reader.Read(chunkBytes);

            cloud[i] = chunkReader.Gaussian;
        }

        return cloud;

        #endregion

    }


    /// <summary>
    /// Deserializes a gaussian splat from a file path pointing to a PLY file.
    /// </summary>
    /// <param name="filePath">The path to the PLY file.</param>
    /// <returns>A cloud of gaussians containing the deserialized data.</returns>
    /// <exception cref="SplatFormatException">Thrown when decoding the gaussian splat fails.</exception>
    public static GaussianCloud FromPly(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        return FromPly(stream);
    }




    /// <summary>
    /// Serializes this gaussian cloud to a stream and encodes it in the PLY file format.
    /// </summary>
    /// <param name="stream">The stream to serialize this gaussian cloud to.</param>
    public static void ToPly(this GaussianCloud gaussians, Stream stream)
    {
        using BinaryWriter writer = new(stream);

        int num = gaussians.Count;
        int shDim = SplatSerializationHelpers.DimForDegree(gaussians.ShDegree);
        int shValCount = shDim * 3;
        int splatChunkCount = 17 + shValCount;



        writer.WriteLine("ply");
        writer.WriteLine("format binary_little_endian 1.0");
        writer.WriteLine("element vertex " + num);
        writer.WriteLine("property float x");
        writer.WriteLine("property float y");
        writer.WriteLine("property float z");
        writer.WriteLine("property float scale_0");
        writer.WriteLine("property float scale_1");
        writer.WriteLine("property float scale_2");
        writer.WriteLine("property float rot_0");
        writer.WriteLine("property float rot_1");
        writer.WriteLine("property float rot_2");
        writer.WriteLine("property float rot_3");
        writer.WriteLine("property float opacity");
        writer.WriteLine("property float f_dc_0");
        writer.WriteLine("property float f_dc_1");
        writer.WriteLine("property float f_dc_2");

        for (int i = 0; i < shValCount; i++)
            writer.WriteLine("property float f_rest_" + i);

        writer.WriteLine("property float nx");
        writer.WriteLine("property float ny");
        writer.WriteLine("property float nz");
        writer.WriteLine("end_header");


        byte[] splatChunk = new byte[splatChunkCount * Unsafe.SizeOf<float>()];
        Span<float> curChunk = MemoryMarshal.Cast<byte, float>(splatChunk);

        Span<int> shIdx = stackalloc int[shValCount];
        for (int i = 0; i < shValCount; i++)
            shIdx[i] = 14 + i;

        SplatChunkReaderWriter chunkWriter = new(
            curChunk,
            [ 0, 1, 2 ],
            [ 3, 4, 5 ],
            [ 6, 7, 8, 9 ],
            10,
            [ 11, 12, 13 ],
            shIdx,
            shDim
        );

        for (int i = 0; i < num; i++)
        {
            chunkWriter.Gaussian = gaussians[i];
            writer.Write(splatChunk);
        }
    }



    /// <summary>
    /// Serializes this gaussian cloud to file at the specified path in the PLY file format.
    /// </summary>
    /// <param name="filePath">The path to the file where this gaussian cloud will be written to.</param>
    public static void ToPly(this GaussianCloud gaussians, string filePath)
    {
        using FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

        ToPly(gaussians, stream);
    }




    public static PackedGaussianCloud FromSpz(Stream stream)
    {
        GZipStream decompressor = new(stream, CompressionMode.Decompress);
        BinaryReader reader = new(decompressor);

        
        PackedGaussiansHeader header = reader.ReadSpzHeader();
        int numPoints = (int)header.NumPoints;
        int shDim = SplatSerializationHelpers.DimForDegree(header.ShDegree);
        int shCoefficients = shDim * 3;
        PackedGaussianCloud cloud = new(numPoints, shDim, header.FractionalBits, (header.Flags & 1) != 0);


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



    public static void TransposeHarmonics(this Span<Gaussian> gaussians, int shDim)
    {

    }
}



public static class StreamHelpers
{
    const int READ_CHUNK_COUNT = 512;
    private static readonly ArrayPool<byte> pool = ArrayPool<byte>.Create();



    // public static void Read(this Stream stream, Span<byte> bytes)
    // {
    //     byte[] buffer = pool.Rent(READ_CHUNK_COUNT);
    //     Span<byte> bufferSpan = buffer;

    //     int len = bytes.Length;
    //     int curIndex = 0;

    //     while (curIndex < len)
    //     {
    //         int bytesRead = stream.Read(buffer, 0, READ_CHUNK_COUNT);
    //         bufferSpan[..bytesRead].CopyTo(bytes[curIndex..]);
    //         curIndex += bytesRead;
    //     }

    //     pool.Return(buffer);
    // }


    public static void Read(this BinaryReader reader, Span<byte> bytes)
    {
        byte[] buffer = pool.Rent(READ_CHUNK_COUNT);
        Span<byte> bufferSpan = buffer;

        int len = bytes.Length;
        int curIndex = 0;

        while (curIndex < len)
        {
            Span<byte> byteSlice = bytes[curIndex..];
            int curLength = Math.Min(byteSlice.Length, READ_CHUNK_COUNT);
            reader.Read(buffer, 0, curLength);
            bufferSpan[..curLength].CopyTo(byteSlice);
            curIndex += curLength;
        }

        pool.Return(buffer);
    }
}