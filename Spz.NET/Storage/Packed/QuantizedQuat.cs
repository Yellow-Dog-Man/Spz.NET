using System.Numerics;
using System.Runtime.CompilerServices;
using Spz.NET.Helpers;

namespace Spz.NET;

public readonly struct QuantizedQuat
{
    public Quaternion Quat
    {
        get
        {
            Vector3 xyz = (new Vector3(X, Y, Z) * (1f / 127.5f)) + new Vector3(-1f);
            return new(xyz, (float)Math.Sqrt(Math.Max(0f, 1f - xyz.LengthSquared())));
        }
    }


    public readonly byte X;
    public readonly byte Y;
    public readonly byte Z;

    public QuantizedQuat(Quaternion value)
    {
        value = Quaternion.Normalize(value);

        value *= value.W < 0 ? -127.5f : 127.5f;
        value += new Quaternion(127.5f, 127.5f, 127.5f, 127.5f);

        X = value.X.ByteClamp();
        Y = value.Y.ByteClamp();
        Z = value.Z.ByteClamp();
    }


    public override string ToString()
    {
        return Quat.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Quaternion(QuantizedQuat other) => other.Quat;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuantizedQuat(Quaternion other) => new(other);
}
