using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;


public static class SplatSerializationHelper
{
    const string PLY_HEADER = "ply";
    const string SUPPORTED_FORMAT = "format binary_little_endian 1.0";
    const string ELEMENT_VERTICES_MARKER = "element vertex ";
    const string FLOAT_PROPERTY_MARKER = "property float ";


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



    public static GaussianCloud LoadSplatFromPly(Stream stream)
    {
        using StreamReader reader = new(stream);

        #region Splat validation

        string header = reader.ReadLine();
        if (header != PLY_HEADER)
            throw new SplatLoadException("The input file doesn't appear to be a ply file.");
        
        string format = reader.ReadLine();
        if (format != SUPPORTED_FORMAT)
            throw new SplatLoadException($"Unsupported ply format: {format}");
        
        string pointCountMarker = reader.ReadLine();
        if (!pointCountMarker.Contains(ELEMENT_VERTICES_MARKER))
            throw new SplatLoadException("");
        
        string pointCountStr = pointCountMarker.Substring(ELEMENT_VERTICES_MARKER.Length);

        if (!int.TryParse(pointCountStr, out int numPoints))
            throw new SplatLoadException($"Unable to parse point count from: \"{pointCountMarker}\"");
        
        if (numPoints <= 0 || numPoints > 10 * 1024 * 1024)
            throw new SplatLoadException($"Invalid vertex count: {numPoints}");
        
        #endregion



        #region Property decoding

        Dictionary<string, int> fields = [];

        int index(string key)
        {
            if (!fields.TryGetValue(key, out int value))
                throw new SplatLoadException($"Attribute was missing: {key}");
            
            return value;
        }
        


        for (int i = 0; /*End condition depends on file contents*/ ; i++)
        {
            string curLine = reader.ReadLine();


            if (curLine == "end_header")
                break;

            if (!curLine.StartsWith(FLOAT_PROPERTY_MARKER))
                throw new SplatLoadException($"Unsupported property data type: {curLine}");


            string name = curLine.Substring(FLOAT_PROPERTY_MARKER.Length);
            fields.Add(name, i);
        }
        int fieldCount = fields.Count;

        
        Span<int> positionIdx = [ index("x"), index("y"), index("z") ];
        Span<int> scaleIdx    = [ index("scale_0"), index("scale_1"), index("scale_2") ];
        Span<int> rotIdx      = [ index("rot_1"), index("rot_2"), index("rot_3"), index("rot_0") ];
        Span<int> alphaIdx    = [ index("opacity") ];
        Span<int> colorIdx    = [ index("f_dc_0"), index("f_dc_1"), index("f_dc_2") ];
        Span<int> shIdx       = stackalloc int[45];
        int shIdxLen;

        for (shIdxLen = 0; shIdxLen < 45; shIdxLen++)
        {
            try
            {
                shIdx[shIdxLen] = index($"f_rest_{shIdxLen}");
            }
            catch (SplatLoadException)
            {
                break;
            }
        }
        shIdx = shIdx.Slice(0, shIdxLen);


        int shDim = shIdx.Length / 3;
        
        #endregion



        #region Splat decoding
        
        byte[] valueBytes = new byte[numPoints * Unsafe.SizeOf<float>() * fieldCount];
        stream.Read(valueBytes, 0, valueBytes.Length);
        Span<float> values = MemoryMarshal.Cast<byte, float>(valueBytes);

        GaussianCloud cloud = new(numPoints, shDim, false);



        for (int i = 0; i < values.Length; i += fieldCount)
        {
            Vector3 pos = new(values[i + positionIdx[0]], values[i + positionIdx[1]], values[i + positionIdx[2]]);
            Vector3 scale = new(values[i + scaleIdx[0]], values[i + scaleIdx[1]], values[i + scaleIdx[2]]);
            Quaternion rotation = new(values[i + rotIdx[0]], values[i + rotIdx[1]], values[i + rotIdx[2]], values[i + rotIdx[3]]);
            float alpha = values[i + alphaIdx[0]];
            Vector3 color = new(values[i + colorIdx[0]], values[i + colorIdx[1]], values[i + colorIdx[2]]);

            Gaussian curGauss = new(pos, scale, rotation, alpha, color);
            cloud.AppendGaussian(curGauss);
        }

        return cloud;
        #endregion

    }


    public static GaussianCloud LoadSplatFromPly(string filePath)
    {

    }


    // public static PackedGaussians PackGaussians(GaussianCloud gaussians)
    // {
    //     int numPoints = gaussians.NumPoints;
    //     int shDim = DimForDegree(gaussians.ShDegree);

    //     PackedGaussians packed = new()
    //     {

    //     }
    // }


}


public class SplatLoadException : Exception
{
    public SplatLoadException(string? msg) : base(msg) { }
}


public static class SplatMathHelper
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int Clamp(int value, int min, int max)
    {
        return Math.Min(Math.Max(min, value), max);
    }



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte QuantizeSH(float x, int bucketSize)
    {
        int q = (int)(Math.Round(x * 128f) + 128f);

        q = (q + bucketSize / 2) / bucketSize * bucketSize;
        return (byte)Clamp(q, 0, 255);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float UnquantizeSH(byte x) => (x - 128f) / 128f;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float Sigmoid(float x) => 1f / (1f + (float)Math.Exp(-x));

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float InvSigmoid(float x) => (float)Math.Log(x / (1f - x));
}


