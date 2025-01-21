using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SharPZ;


public static class SplatSerializationHelpers
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
        PackedGaussiansHeader readHeader = new(
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadUInt32(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte(),
            reader.ReadByte()
        );

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
        int shDim = DimForDegree(cloud.ShDegree);
        PackedGaussianCloud packed = new(cloud.Count, shDim, fractionalBits, cloud.Antialiased);

        for (int i = 0; i < cloud.Count; i++)
        {
            packed.positions[i] = cloud.positions[i].ToFixed(fractionalBits);
            packed.alphas[i] = cloud.alphas[i];
            packed.colors[i] = cloud.colors[i];
            packed.scales[i] = cloud.scales[i];
            packed.rotations[i] = cloud.rotations[i];


            var harmonic = cloud.sh.GetRowSpan(i);
            var packedHarmonic = packed.sh.GetRowSpan(i);

            harmonic.Quantize(packedHarmonic);
        }

        return packed;
    }



    



    public static GaussianCloud Unpack(this PackedGaussianCloud packed, int fractionalBits = PackedGaussian.DEFAULT_FRACTIONAL_BITS)
    {
        GaussianCloud unpacked = new(packed.Count, DimForDegree(packed.ShDegree), packed.Antialiased);

        for (int i = 0; i < packed.Count; i++)
        {
            unpacked.positions[i] = packed.positions[i].ToVector3(fractionalBits);
            unpacked.alphas[i] = packed.alphas[i];
            unpacked.colors[i] = packed.colors[i];
            unpacked.scales[i] = packed.scales[i];
            unpacked.rotations[i] = packed.rotations[i];


            var harmonic = unpacked.sh.GetRowSpan(i);
            var packedHarmonic = packed.sh.GetRowSpan(i);

            packedHarmonic.Unquantize(harmonic);
        }

        return unpacked;
    }
}
