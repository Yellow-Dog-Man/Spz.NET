using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace SharPZ;


public static class SplatSerializationHelper
{
    const string PLY_HEADER = "ply";
    const string SUPPORTED_FORMAT = "format binary_little_endian 1.0";
    const string ELEMENT_VERTICES_MARKER = "element vertex ";


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


    public static void WriteLine(this BinaryWriter writer, string text)
    {
        byte[] textBytes = Encoding.UTF8.GetBytes(text + '\n');
        writer.Write(textBytes, 0, textBytes.Length);
    }


    public static void WriteVector3(this BinaryWriter writer, in Vector3 vec)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
        writer.Write(vec.Z);
    }

    public static void WriteQuaternionWXYZ(this BinaryWriter writer, in Quaternion quat)
    {
        writer.Write(quat.W);
        writer.Write(quat.X);
        writer.Write(quat.Y);
        writer.Write(quat.Z);
    }


    public static int ValidatePlySplat(BinaryReader reader)
    {
        string header = reader.ReadLine();
        if (header != PLY_HEADER)
            throw new SplatFormatException("The input file doesn't appear to be a ply file.");
        
        string format = reader.ReadLine();
        if (format != SUPPORTED_FORMAT)
            throw new SplatFormatException($"Unsupported ply format: {format}");
        
        string pointCountMarker = reader.ReadLine();
        if (!pointCountMarker.Contains(ELEMENT_VERTICES_MARKER))
            throw new SplatFormatException($"Couldn't determine element vertices from: {pointCountMarker}");
        
        string pointCountStr = pointCountMarker.Substring(ELEMENT_VERTICES_MARKER.Length);

        if (!int.TryParse(pointCountStr, out int numPoints))
            throw new SplatFormatException($"Unable to parse point count from: \"{pointCountMarker}\"");
        
        if (numPoints <= 0 || numPoints > 10 * 1024 * 1024)
            throw new SplatFormatException($"Invalid vertex count: {numPoints}");
        

        return numPoints;
    }

    // public static void WriteHarmonics(this BinaryWriter writer, in GaussianHarmonics harmonics, int dimensions)
    // {
    //     for (int i = 0; i < dimensions; i++)
    //     {
    //         Vector3 harmonicVec = new(harmonics[i])
    //     }
    // }


    // public static GaussianHarmonics ReadHarmonics(this BinaryReader reader, int shDim)
    // {
    //     GaussianHarmonics sh = new();
    //     for (int j = 0; j < shDim; j++)
    //     {
    //         Vector3 harmonic = new(
    //             values[i + shIdx[j]],
    //             values[i + shIdx[j + shDim]],
    //             values[i + shIdx[j + 2 * shDim]]);
            
    //         sh[j] = harmonic;
    //     }
    //     return 
    // }
}


