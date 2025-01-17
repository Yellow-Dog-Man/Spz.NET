using System.Numerics;

namespace SharPZ;

public readonly struct Gaussian(in Vector3 pos, in Vector3 scale, in Quaternion rotation, float alpha, in Vector3 color, in GaussianHarmonics sh)
{
    public readonly Vector3 Position = pos;
    public readonly Vector3 Scale = scale;
    public readonly Quaternion Rotation = rotation;
    public readonly float Alpha = alpha;
    public readonly Vector3 Color = color;
    public readonly GaussianHarmonics Sh = sh;
}



public readonly struct PackedGaussian
{

}


public readonly struct Fixed24Vector3
{
    
}