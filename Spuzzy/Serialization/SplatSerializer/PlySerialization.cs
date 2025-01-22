using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Spuzzy.Helpers;
using Spuzzy.Structs;

namespace Spuzzy.Serialization;


/// <summary>
/// Functions for serializing and deserializing gaussian splats.
/// </summary>
public static partial class SplatSerializer
{
    public const string PLY_HEADER = "ply";
    public const string END_HEADER = "end_header";
    public const string FORMAT_MARKER = "format ";
    public const string FLOAT_PROPERTY_MARKER = "property float ";
    public const string ELEMENT_VERTICES_MARKER = "element vertex ";
    public const string SUPPORTED_FORMAT = "binary_little_endian 1.0";



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



    /// <summary>
    /// Deserializes a gaussian splat from a stream providing a PLY file.
    /// </summary>
    /// <param name="stream">The stream to read the PLY file from.</param>
    /// <returns>A cloud of gaussians containing the deserialized data.</returns>
    /// <exception cref="SplatFormatException">Thrown when decoding the gaussian splat fails.</exception>
    public static GaussianCloud FromPly(Stream stream)
    {
        // Create a binary reader to aid in reading the stream
        using BinaryReader reader = new(stream);

        // Validate the PLY file and return the number of points it has in it.
        int numPoints = ValidatePlySplat(reader);

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
        GaussianCloud cloud = new(numPoints, shDim);

        // Create a reader/writer with the indicies of each gaussian's properties within the chunk.
        PlyChunker chunkReader = new(
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
    /// Serializes this gaussian splat to a stream and encodes it in the PLY file format.
    /// </summary>
    /// <param name="gaussians">The compressed cloud of gaussians to serialize.</param>
    /// <param name="stream">The stream to serialize this gaussian cloud to.</param>
    public static void ToPly(this GaussianCloud gaussians, Stream stream)
    {
        using BinaryWriter writer = new(stream);

        int num = gaussians.Count;
        int shDim = SplatMathHelpers.DimForDegree(gaussians.ShDegree);
        int shValCount = shDim * 3;
        int splatChunkCount = 17 + shValCount;


        // Write header info.
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


        Span<float> curChunk = stackalloc float[splatChunkCount];
        Span<byte> splatChunk = MemoryMarshal.Cast<float, byte>(curChunk);

        Span<int> shIdx = stackalloc int[shValCount];
        for (int i = 0; i < shValCount; i++)
            shIdx[i] = 14 + i;

        PlyChunker chunkWriter = new(
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
    /// Serializes this gaussian splat to file at the specified path in the PLY file format.
    /// </summary>
    /// <param name="gaussians">The cloud of gaussians to serialize.</param>
    /// <param name="filePath">The path to the file where this gaussian cloud will be written to.</param>
    public static void ToPly(this GaussianCloud gaussians, string filePath)
    {
        using FileStream stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

        ToPly(gaussians, stream);
    }
}
