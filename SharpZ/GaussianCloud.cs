using System.Numerics;

namespace SharPZ;

public class GaussianCloud(int numPoints, int shDim, bool antialiased = false)
{
    public readonly int NumPoints = numPoints;
    public int CurPoints { get; private set; }
    public readonly int ShDegree = SplatSerializationHelper.DegreeForDim(shDim);
    public readonly bool Antialiased = antialiased;


    public readonly Vector3[] Positions = new Vector3[numPoints];
    public readonly Vector3[] Scales = new Vector3[numPoints];
    public readonly Quaternion[] Rotations = new Quaternion[numPoints];
    public readonly float[] Alphas = new float[numPoints];
    public readonly Vector3[] Colors = new Vector3[numPoints];
    public readonly float[] Sh;


    public void AppendGaussian(in Gaussian gaussian)
    {
        int curPoint = CurPoints;

        Positions[curPoint] = gaussian.Position;
        Scales[curPoint] = gaussian.Scale;
        Rotations[curPoint] = gaussian.Rotation;
        Alphas[curPoint] = gaussian.Alpha;
        Colors[curPoint] = gaussian.Color;

        CurPoints++;
    }
}



public readonly struct Gaussian(in Vector3 pos, in Vector3 scale, in Quaternion rotation, float alpha, in Vector3 color)
{
    public readonly Vector3 Position;
    public readonly Vector3 Scale;
    public readonly Quaternion Rotation;
    public readonly float Alpha;
    public readonly Vector3 Color;
    // public readonly 
}


public readonly struct Fixed24
{
    const uint FRACTIONAL_BITS_MASK = 0xff000000;
    public readonly byte FractionalBits => (byte)((_value & FRACTIONAL_BITS_MASK) >> 24);
    private readonly int _value;

    
    Fixed24(int value, byte fractionalBits)
    {
        _value = (fractionalBits << 24) + (int)(value & ~FRACTIONAL_BITS_MASK);
    }

    public static Fixed24 FromFloat(float x, byte fractionalBits)
    {
        int whole = (int)Math.Round(x * (1 << fractionalBits));


        // Console.WriteLine((int)whole >> fractionalBits);
        return new(whole, fractionalBits);
    }


    public override string ToString()
    {
        return $"{(_value & ~FRACTIONAL_BITS_MASK) >> FractionalBits}.{((((1 << FractionalBits) - 1) & _value) << FractionalBits) / (1 << FractionalBits)}";
    }
}


public readonly struct ShCoefficient
{

}