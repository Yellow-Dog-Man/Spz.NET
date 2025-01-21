using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharPZ;

public readonly struct FixedVector3(in Vector3 value, int fractionalBits)
{
    public readonly Fixed24 X = value.X.ToFixed(fractionalBits);
    public readonly Fixed24 Y = value.Y.ToFixed(fractionalBits);
    public readonly Fixed24 Z = value.Z.ToFixed(fractionalBits);



    public Vector3 ToVector3(int fractionalBits) => new(X.ToFloat(fractionalBits), Y.ToFloat(fractionalBits), Z.ToFloat(fractionalBits));
}
