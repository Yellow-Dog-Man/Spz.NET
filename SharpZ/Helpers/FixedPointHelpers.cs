using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharPZ;

public static class FixedPointHelpers
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed24 ToFixed(this float value, int fractionalBits) => new(value, fractionalBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedVector3 ToFixed(this Vector3 value, int fractionalBits) => new(value, fractionalBits);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ByteClamp(this float value) => (byte)Math.Min(Math.Max(0d, Math.Round(value)), 255d);
}
