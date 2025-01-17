using System.Collections;
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


    public void ToPly(string filePath)
    {
        using FileStream stream = File.OpenWrite(filePath);

        ToPly(stream);
    }


}
