using System.Numerics;

namespace SharPZ;


public interface IGaussian
{
    bool Compressed { get; }

    Vector3 Position { get; }
    Vector3 Scale { get; }
    Quaternion Rotation { get; }
    float Alpha { get; }
    Vector3 Color { get; }
    GaussianHarmonics<float> Sh { get; }
}


public readonly struct Gaussian : IGaussian
{
    public readonly bool Compressed => false;

    public Gaussian(in PackedGaussian packed, int fractionalBits)
    {
        Position = packed.PackedPosition.ToVector3(fractionalBits);
        Scale = packed.PackedScale;
        Rotation = packed.PackedRotation;
        Alpha = packed.PackedAlpha;
        Color = packed.PackedColor;
        Sh = packed.Sh;
    }

    public Gaussian(in Vector3 pos, in Vector3 scale, in Quaternion rotation, float alpha, in Vector3 color, in GaussianHarmonics<float> sh)
    {
        Position = pos;
        Scale = scale;
        Rotation = rotation;
        Alpha = alpha;
        Color = color;
        Sh = sh;
    }


    public readonly PackedGaussian Pack(int fractionalBits) => new(this, fractionalBits);

    public readonly Vector3 Position { get; }
    public readonly Vector3 Scale { get; }
    public readonly Quaternion Rotation { get; }
    public readonly float Alpha { get; }
    public readonly Vector3 Color { get; }
    public readonly GaussianHarmonics<float> Sh { get; }
}
