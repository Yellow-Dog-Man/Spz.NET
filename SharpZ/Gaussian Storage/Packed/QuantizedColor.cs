using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharPZ;

public readonly struct QuantizedColor
{
    public readonly Vector3 Color => (new Vector3(X, Y, Z) / 255f) - new Vector3(0.5f);
    const float COLOR_SCALE = 0.15f;
    public readonly byte X;
    public readonly byte Y;
    public readonly byte Z;

    
    public QuantizedColor(Vector3 color)
    {
        color *= new Vector3(COLOR_SCALE * 255f) + new Vector3(0.5f * 255f);
        X = (byte)color.X;
        Y = (byte)color.Y;
        Z = (byte)color.Z;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(QuantizedColor other) => other.Color;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuantizedColor(Vector3 other) => new(other);
}