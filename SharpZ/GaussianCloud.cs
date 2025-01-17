using System.Collections;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SharPZ;

public class GaussianCloud(int capacity, int shDim, bool antialiased = false) : IReadOnlyList<Gaussian>
{
    const string END_HEADER = "end_header";
    const string FLOAT_PROPERTY_MARKER = "property float ";



    public Gaussian this[int index]
    {
        get => gaussians[index];
        set => gaussians[index] = value;
    }


    public int Count => gaussians.Length;
    public bool IsReadOnly => true;


    public readonly int ShDegree = SplatSerializationHelper.DegreeForDim(shDim);
    public readonly bool Antialiased = antialiased;


    private readonly Gaussian[] gaussians = new Gaussian[capacity];

    public bool Contains(Gaussian gaussian) => gaussians.Contains(gaussian);

    public void CopyTo(Gaussian[] array, int arrayIndex) => gaussians.CopyTo(array, arrayIndex);


    IEnumerator<Gaussian> IEnumerable<Gaussian>.GetEnumerator() => (IEnumerator<Gaussian>)gaussians.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => gaussians.GetEnumerator();



    public static GaussianCloud FromPly(Stream stream)
    {
        using BinaryReader reader = new(stream);
        int numPoints = SplatSerializationHelper.ValidatePlySplat(reader);

        #region Property decoding

        Dictionary<string, int> fields = [];

        int index(string key)
        {
            if (!fields.TryGetValue(key, out int value))
                throw new SplatFormatException($"Attribute was missing: {key}");

            return value;
        }


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
        
        int splatChunkCount = fieldCount * Unsafe.SizeOf<float>();
        byte[] splatChunk = new byte[splatChunkCount];


        GaussianCloud cloud = new(numPoints, shDim, false);
        Span<float> curChunk = MemoryMarshal.Cast<byte, float>(splatChunk);

        SplatChunkReaderWriter chunkReader = new(
            curChunk,
            [ index("x"), index("y"), index("z") ],
            [ index("scale_0"), index("scale_1"), index("scale_2") ],
            [ index("rot_0"), index("rot_1"), index("rot_2"), index("rot_3") ],
            index("opacity"),
            [ index("f_dc_0"), index("f_dc_1"), index("f_dc_2") ],
            shIdx,
            shDim
        );



        for (int i = 0; i < numPoints; i++)
        {
            reader.Read(splatChunk, 0, splatChunkCount);

            cloud[i] = chunkReader.Gaussian;
        }

        return cloud;

        #endregion

    }


    public static GaussianCloud FromPly(string filePath)
    {
        using FileStream stream = File.OpenRead(filePath);
        return FromPly(stream);
    }


    public void ToPly(Stream stream)
    {
        using BinaryWriter writer = new(stream);

        int num = Count;
        int shDim = SplatSerializationHelper.DimForDegree(ShDegree);
        int shValCount = shDim * 3;
        int splatChunkCount = 17 + shValCount;



        writer.WriteLine("ply");
        writer.WriteLine("format binary_little_endian 1.0");
        writer.WriteLine("element vertex " + num);
        writer.WriteLine("property float x");
        writer.WriteLine("property float y");
        writer.WriteLine("property float z");
        writer.WriteLine("property float nx");
        writer.WriteLine("property float ny");
        writer.WriteLine("property float nz");
        writer.WriteLine("property float f_dc_0");
        writer.WriteLine("property float f_dc_1");
        writer.WriteLine("property float f_dc_2");

        for (int i = 0; i < shValCount; i++)
            writer.WriteLine("property float f_rest_" + i);

        writer.WriteLine("property float opacity");
        writer.WriteLine("property float scale_0");
        writer.WriteLine("property float scale_1");
        writer.WriteLine("property float scale_2");
        writer.WriteLine("property float rot_0");
        writer.WriteLine("property float rot_1");
        writer.WriteLine("property float rot_2");
        writer.WriteLine("property float rot_3");
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


    public void ToPly(string filePath)
    {
        using FileStream stream = File.OpenWrite(filePath);

        ToPly(stream);
    }


}


internal readonly ref struct SplatChunkReaderWriter(
    Span<float> chunk,
    Span<int> positionIdx,
    Span<int> scaleIdx,
    Span<int> rotIdx,
    int alphaIdx,
    Span<int> colorIdx,
    Span<int> shIdx,
    int shDim)
{
    readonly Span<float> chunk = chunk;
    readonly Span<int> positionIdx = positionIdx;
    readonly Span<int> scaleIdx = scaleIdx;
    readonly Span<int> rotIdx = rotIdx;
    readonly Span<int> colorIdx = colorIdx;
    readonly Span<int> shIdx = shIdx;


    public readonly Vector3 Position
    {
        get => new(chunk[positionIdx[0]], chunk[positionIdx[1]], chunk[positionIdx[2]]);
        set
        {
            chunk[positionIdx[0]] = value.X;
            chunk[positionIdx[1]] = value.Y;
            chunk[positionIdx[2]] = value.Z;
        }
    }
    public readonly Vector3 Scale
    {
        get => new(chunk[scaleIdx[0]], chunk[scaleIdx[1]], chunk[scaleIdx[2]]);
        set
        {
            chunk[scaleIdx[0]] = value.X;
            chunk[scaleIdx[1]] = value.Y;
            chunk[scaleIdx[2]] = value.Z;
        }
    }

    // The quaternion is read as WXYZ, so shove W at the end to encode it as normal.
    public readonly Quaternion Rotation
    {
        get => new(chunk[rotIdx[1]], chunk[rotIdx[2]], chunk[rotIdx[3]], chunk[rotIdx[0]]);
        set
        {
            chunk[rotIdx[1]] = value.X;
            chunk[rotIdx[2]] = value.Y;
            chunk[rotIdx[3]] = value.Z;
            chunk[rotIdx[0]] = value.W;
        }
    }

    public readonly Vector3 Color
    {
        get => new(chunk[colorIdx[0]], chunk[colorIdx[1]], chunk[colorIdx[2]]);
        set
        {
            chunk[colorIdx[0]] = value.X;
            chunk[colorIdx[1]] = value.Y;
            chunk[colorIdx[2]] = value.Z;
        }
    }

    public readonly float Alpha
    {
        get => chunk[alphaIdx];
        set => chunk[alphaIdx] = value;
    }

    public readonly GaussianHarmonics Sh
    {
        get
        {
            GaussianHarmonics sh = new();

            for (int i = 0; i < shDim; i++)
            {
                sh[i] = new(
                    chunk[shIdx[i]],
                    chunk[shIdx[i + shDim]],
                    chunk[shIdx[i + 2 * shDim]]);
            }

            return sh;
        }

        set
        {
            for (int i = 0; i < shDim; i++)
            {
                Vector3 harmonic = value[i];

                chunk[shIdx[i]] = harmonic.X;
                chunk[shIdx[i + shDim]] = harmonic.Y;
                chunk[shIdx[i + 2 * shDim]] = harmonic.Z;
            }
        }
    }


    public readonly Gaussian Gaussian
    {
        get => new(
            Position,
            Scale,
            Rotation,
            Alpha,
            Color,
            Sh
        );

        set
        {
            Position = value.Position;
            Scale = value.Scale;
            Rotation = value.Rotation;
            Alpha = value.Alpha;
            Color = value.Color;
            Sh = value.Sh;
        }
    }
}



public readonly struct Gaussian(in Vector3 pos, in Vector3 scale, in Quaternion rotation, float alpha, in Vector3 color, in GaussianHarmonics sh)
{
    public readonly Vector3 Position = pos;
    public readonly Vector3 Scale = scale;
    public readonly Quaternion Rotation = rotation;
    public readonly float Alpha = alpha;
    public readonly Vector3 Color = color;
    public readonly GaussianHarmonics Sh = sh;
}



public struct GaussianHarmonics(
    Vector3 c0,
    Vector3 c1,
    Vector3 c2,
    Vector3 c3,
    Vector3 c4,
    Vector3 c5,
    Vector3 c6,
    Vector3 c7,
    Vector3 c8,
    Vector3 c9,
    Vector3 c10,
    Vector3 c11,
    Vector3 c12,
    Vector3 c13,
    Vector3 c14,
    Vector3 c15)
{

    public Vector3 this[int index]
    {
        readonly get => index switch
        {
            0  => Coefficient0,
            1  => Coefficient1,
            2  => Coefficient2,
            3  => Coefficient3,
            4  => Coefficient4,
            5  => Coefficient5,
            6  => Coefficient6,
            7  => Coefficient7,
            8  => Coefficient8,
            9  => Coefficient9,
            10 => Coefficient10,
            11 => Coefficient11,
            12 => Coefficient12,
            13 => Coefficient13,
            14 => Coefficient14,
            15 => Coefficient15,
            _  => throw new NotSupportedException($"Spherical harmonic degree not supported: {index}")
        };
        set
        {
            switch (index)
            {
                case 0:
                    Coefficient0 = value;
                    break;

                case 1:
                    Coefficient1 = value;
                    break;

                case 2:
                    Coefficient2 = value;
                    break;

                case 3:
                    Coefficient3 = value;
                    break;

                case 4:
                    Coefficient4 = value;
                    break;

                case 5:
                    Coefficient5 = value;
                    break;

                case 6:
                    Coefficient6 = value;
                    break;

                case 7:
                    Coefficient7 = value;
                    break;

                case 8:
                    Coefficient8 = value;
                    break;

                case 9:
                    Coefficient9 = value;
                    break;

                case 10:
                    Coefficient10 = value;
                    break;

                case 11:
                    Coefficient11 = value;
                    break;

                case 12:
                    Coefficient12 = value;
                    break;

                case 13:
                    Coefficient13 = value;
                    break;

                case 14:
                    Coefficient14 = value;
                    break;

                case 15:
                    Coefficient15 = value;
                    break;
                
                default:
                    throw new NotSupportedException($"Cannot set spherical harmonic degree: {index}");

            }
        }
    }

    public Vector3 Coefficient0  = c0;
    public Vector3 Coefficient1  = c1;
    public Vector3 Coefficient2  = c2;
    public Vector3 Coefficient3  = c3;
    public Vector3 Coefficient4  = c4;
    public Vector3 Coefficient5  = c5;
    public Vector3 Coefficient6  = c6;
    public Vector3 Coefficient7  = c7;
    public Vector3 Coefficient8  = c8;
    public Vector3 Coefficient9  = c9;
    public Vector3 Coefficient10 = c10;
    public Vector3 Coefficient11 = c11;
    public Vector3 Coefficient12 = c12;
    public Vector3 Coefficient13 = c13;
    public Vector3 Coefficient14 = c14;
    public Vector3 Coefficient15 = c15;
}