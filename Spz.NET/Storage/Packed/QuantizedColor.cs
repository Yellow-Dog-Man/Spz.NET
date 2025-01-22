using System.Numerics;
using System.Runtime.CompilerServices;
using Spz.NET.Helpers;

namespace Spz.NET;

public readonly struct QuantizedColor
{
    const float COLOR_SCALE = 0.15f;


    public readonly Vector3 Color => ((new Vector3(X, Y, Z) / 255f) - new Vector3(0.5f)) / COLOR_SCALE;
    public readonly byte X;
    public readonly byte Y;
    public readonly byte Z;

    
    public QuantizedColor(Vector3 color)
    {
        color *= new Vector3(COLOR_SCALE * 255f);
        color += new Vector3(0.5f * 255f);

        X = color.X.ByteClamp();
        Y = color.Y.ByteClamp();
        Z = color.Z.ByteClamp();
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(QuantizedColor other) => other.Color;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuantizedColor(Vector3 other) => new(other);
}