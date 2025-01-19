using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharPZ;

public readonly struct QuantizedScale
{
    public Vector3 Scale => new Vector3(X, Y, Z) / 16f - new Vector3(10f); 
    public readonly byte X;
    public readonly byte Y;
    public readonly byte Z;


    public QuantizedScale(Vector3 scale)
    {
        scale = (scale + new Vector3(10f)) * 16f;
        X = (byte)scale.X;
        Y = (byte)scale.Y;
        Z = (byte)scale.Z;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Vector3(QuantizedScale other) => other.Scale;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator QuantizedScale(Vector3 other) => new(other);
}
