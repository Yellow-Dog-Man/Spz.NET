using System.Numerics;

namespace Spz.NET;


public readonly struct Gaussian(in Vector3 pos, in Vector3 scale, in Quaternion rotation, float alpha, in Vector3 color, in GaussianHarmonics sh) : IEquatable<Gaussian>
{
    public readonly bool Compressed => false;

    public readonly Vector3 Position { get; } = pos;
    public readonly Vector3 Scale { get; } = scale;
    public readonly Quaternion Rotation { get; } = rotation;
    public readonly float Alpha { get; } = alpha;
    public readonly Vector3 Color { get; } = color;
    public readonly GaussianHarmonics Sh { get; } = sh;


    public bool Equals(Gaussian other)
    {
        return
            Position == other.Position &&
            Scale    == other.Scale &&
            Rotation == other.Rotation &&
            Alpha    == other.Alpha &&
            Color    == other.Color &&
            Sh       == other.Sh;
    }



    public static bool operator ==(in Gaussian left, in Gaussian right) => left.Equals(right);


    public static bool operator !=(in Gaussian left, in Gaussian right) => !left.Equals(right);
}
