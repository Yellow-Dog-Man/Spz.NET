using System.Diagnostics;
using System.IO.Compression;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SharPZ;


public static class SplatSerializationHelper
{
    const string PLY_HEADER = "ply";
    const string FORMAT_MARKER = "format ";
    const string SUPPORTED_FORMAT = "binary_little_endian 1.0";
    const string ELEMENT_VERTICES_MARKER = "element vertex ";

    const int SPZ_MAX_POINTS = 10000000;


    public static int DimForDegree(int degree)
    {
        return degree switch
        {
            0 => 0,
            1 => 3,
            2 => 8,
            3 => 15,
            _ => throw new NotImplementedException($"Unsupported SH degree: {degree}")
        };
    }



    public static int DegreeForDim(int dim)
    {
        if (dim < 3)
            return 0;

        if (dim < 8)
            return 1;

        if (dim < 15)
            return 2;

        return 3;
    }



    // TODO: Make this less bad.
    public static string ReadLine(this BinaryReader reader)
    {
        StringBuilder builder = new();
        for (;;)
        {
            char curChar = (char)reader.ReadByte();
            if (curChar == '\n')
                break;

            builder.Append(curChar);
        }

        return builder.ToString();
    }

    public static string? GetLine(this BinaryReader reader, string text, bool exactMatch = true)
    {
        string curLine;

        do
        {
            try
            {
                curLine = reader.ReadLine();
            }
            catch (EndOfStreamException)
            {
                return "";
            }
        }
        while(exactMatch ? curLine != text : !curLine.Contains(text));

        return curLine;
    }


    public static void WriteLine(this BinaryWriter writer, string text)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text + '\n');
        writer.Write(textBytes, 0, textBytes.Length);
    }


    public static int ValidatePlySplat(BinaryReader reader)
    {
        string? header = reader.GetLine(PLY_HEADER);
        if (header == null)
            throw new SplatFormatException("The input file doesn't appear to be a PLY file.");
        
        #if DEBUG
        Console.WriteLine($"DEBUG: PLY header: {header}");
        #endif

        string? formatLine = reader.GetLine(FORMAT_MARKER, false);
        if (formatLine == null)
            throw new SplatFormatException("Unable to determine format of PLY file.");

        #if DEBUG
        Console.WriteLine($"DEBUG: PLY format: {formatLine}");
        #endif

        string format = formatLine.Substring(FORMAT_MARKER.Length);
        if (format != SUPPORTED_FORMAT)
            throw new SplatFormatException($"Only PLY files in the \"{SUPPORTED_FORMAT}\" format can be used, invalid format: {format}");
        
        string? pointCountMarker = reader.GetLine(ELEMENT_VERTICES_MARKER, false);
        if (pointCountMarker == null)
            throw new SplatFormatException($"Couldn't determine element vertices from: {pointCountMarker}");
        
        string pointCountStr = pointCountMarker.Substring(ELEMENT_VERTICES_MARKER.Length);

        if (!int.TryParse(pointCountStr, out int numPoints))
            throw new SplatFormatException($"Unable to parse point count from: \"{pointCountMarker}\"");
        
        if (numPoints <= 0 || numPoints > 10 * 1024 * 1024)
            throw new SplatFormatException($"Invalid vertex count: {numPoints}");
        

        return numPoints;
    }



    public static PackedGaussiansHeader ReadSpzHeader(this BinaryReader reader)
    {
        byte[] header = new byte[Unsafe.SizeOf<PackedGaussiansHeader>()];

        reader.Read(header, 0, header.Length);

        PackedGaussiansHeader readHeader = MemoryMarshal.Cast<byte, PackedGaussiansHeader>(header)[0];

        if (readHeader.Magic != PackedGaussiansHeader.MAGIC)
            throw new SplatFormatException("SPZ header not found.");
        
        if (readHeader.Version != PackedGaussiansHeader.VERSION)
            throw new SplatFormatException($"SPZ version not supported: {readHeader.Version}");
        
        if (readHeader.NumPoints > SPZ_MAX_POINTS)
            throw new SplatFormatException($"SPZ has too many points: {readHeader.NumPoints}");
        
        if (readHeader.ShDegree > 3)
            throw new SplatFormatException($"SPZ has unsupported spherical harmonics degree: {readHeader.ShDegree}");
        
        
        return readHeader;
    }



    public static PackedGaussianCloud Pack(this GaussianCloud cloud, int fractionalBits = PackedGaussian.DEFAULT_FRACTIONAL_BITS)
    {
        PackedGaussianCloud packed = new(cloud.Count, DimForDegree(cloud.ShDegree), fractionalBits, cloud.Antialiased);

        for (int i = 0; i < cloud.Count; i++)
        {
            packed[i] = cloud[i].Pack(fractionalBits);
        }

        return packed;
    }



    public static GaussianCloud Unpack(this PackedGaussianCloud cloud)
    {
        GaussianCloud unpacked = new(cloud.Count, DimForDegree(cloud.ShDegree), cloud.Antialiased);

        for (int i = 0; i < cloud.Count; i++)
        {
            unpacked[i] = cloud[i].Unpack(cloud.FractionalBits);
        }

        return unpacked;
    }
}



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
        int numPoints = SplatSerializationHelper.ValidatePlySplat(reader);

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


            string name = curLine.Substring(FLOAT_PROPERTY_MARKER.Length);
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
                shIdx = shIdx.Slice(0, i);
                break;
            }
        }
        int shDim = shIdx.Length / 3;

        #endregion



        #region Splat decoding

        // Make a temporary array to read the file in chunks. Each chunk will be a gaussian.
        int chunkCount = fieldCount * Unsafe.SizeOf<float>();
        byte[] chunkBytes = new byte[chunkCount];
        Span<float> chunk = MemoryMarshal.Cast<byte, float>(chunkBytes);

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
            reader.Read(chunkBytes, 0, chunkCount);

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
        int shDim = SplatSerializationHelper.DimForDegree(gaussians.ShDegree);
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
        using FileStream stream = File.OpenWrite(filePath);

        ToPly(gaussians, stream);
    }




    public static PackedGaussianCloud FromSpz(Stream stream)
    {
        using GZipStream decompressor = new(stream, CompressionMode.Decompress);
        BinaryReader reader = new(decompressor);
        PackedGaussiansHeader header = reader.ReadSpzHeader();
        int numPoints = (int)header.NumPoints;
        PackedGaussianCloud cloud = new(numPoints, SplatSerializationHelper.DimForDegree(header.ShDegree), header.FractionalBits, (header.Flags & 1) != 0);


        // Chunk of the biggest size of the largest data type.
        byte[] spzChunk = new byte[header.NumPoints * Unsafe.SizeOf<QuantizedHarmonics>()];


        int byteCount = numPoints * Unsafe.SizeOf<FixedVector3>();
        reader.Read(spzChunk, 0, byteCount);
        Span<FixedVector3> spzPositions = MemoryMarshal.Cast<byte, FixedVector3>(spzChunk).Slice(0, cloud.positions.Length);
        spzPositions.CopyTo(cloud.positions);
        

        byteCount = numPoints * Unsafe.SizeOf<QuantizedAlpha>();
        reader.Read(spzChunk, 0, byteCount);
        Span<QuantizedAlpha> spzAlphas = MemoryMarshal.Cast<byte, QuantizedAlpha>(spzChunk).Slice(0, cloud.alphas.Length);
        spzAlphas.CopyTo(cloud.alphas);
        

        byteCount = numPoints * Unsafe.SizeOf<QuantizedColor>();
        reader.Read(spzChunk, 0, byteCount);
        Span<QuantizedColor> spzColors = MemoryMarshal.Cast<byte, QuantizedColor>(spzChunk).Slice(0, cloud.colors.Length);
        spzColors.CopyTo(cloud.colors);
        
        byteCount = numPoints * Unsafe.SizeOf<QuantizedScale>();
        reader.Read(spzChunk, 0, byteCount);
        Span<QuantizedScale> spzScales = MemoryMarshal.Cast<byte, QuantizedScale>(spzChunk).Slice(0, cloud.scales.Length);
        spzScales.CopyTo(cloud.scales);

        byteCount = numPoints * Unsafe.SizeOf<QuantizedQuat>();
        reader.Read(spzChunk, 0, byteCount);
        Span<QuantizedQuat> spzRotations = MemoryMarshal.Cast<byte, QuantizedQuat>(spzChunk).Slice(0, cloud.rotations.Length);
        spzRotations.CopyTo(cloud.rotations);

        byteCount = numPoints * Unsafe.SizeOf<QuantizedHarmonics>();
        reader.Read(spzChunk, 0, byteCount);
        Span<QuantizedHarmonics> spzHarmonics = MemoryMarshal.Cast<byte, QuantizedHarmonics>(spzChunk).Slice(0, cloud.sh.Length);
        spzHarmonics.CopyTo(spzHarmonics);

        return cloud;
    }



    public static PackedGaussianCloud FromSpz(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);

        return FromSpz(stream);
    }
}