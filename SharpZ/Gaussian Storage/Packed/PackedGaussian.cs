using System.Diagnostics;
using System.Numerics;

namespace SharPZ;

public readonly struct PackedGaussian : IGaussian
{
    public const int DEFAULT_FRACTIONAL_BITS = 12;

    public readonly bool Compressed => true;
    public readonly int FractionalBits;


    public readonly Vector3 Position => PackedPosition.ToVector3(FractionalBits);
    public readonly Vector3 Scale => PackedScale;
    public readonly Quaternion Rotation => PackedRotation;
    public readonly float Alpha => PackedAlpha;
    public readonly Vector3 Color => PackedColor;
    public readonly GaussianHarmonics Sh => PackedSh;


    public readonly FixedVector3 PackedPosition;
    public readonly QuantizedScale PackedScale;
    public readonly QuantizedQuat PackedRotation;
    public readonly QuantizedAlpha PackedAlpha;
    public readonly QuantizedColor PackedColor;
    public readonly QuantizedHarmonics PackedSh;


    public Gaussian Unpack(int fractionalBits = DEFAULT_FRACTIONAL_BITS) => new(this, fractionalBits);

    public PackedGaussian(in Gaussian gaussian, int fractionalBits = DEFAULT_FRACTIONAL_BITS)
    {
        FractionalBits = fractionalBits;
        PackedPosition = gaussian.Position.ToFixed(fractionalBits);
        PackedScale = gaussian.Scale;
        PackedRotation = gaussian.Rotation;
        PackedAlpha = gaussian.Alpha;
        PackedColor = gaussian.Color;
        PackedSh = gaussian.Sh;
    }


    public PackedGaussian(
        int fractionalBits,
        in FixedVector3 position,
        in QuantizedScale scale,
        in QuantizedQuat rotation,
        in QuantizedAlpha alpha,
        in QuantizedColor color,
        in QuantizedHarmonics harmonics)
    {
        FractionalBits = fractionalBits;
        PackedPosition = position;
        PackedScale = scale;
        PackedRotation = rotation;
        PackedAlpha = alpha;
        PackedColor = color;
        PackedSh = harmonics;
    }
}
