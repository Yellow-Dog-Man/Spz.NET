using System.Numerics;
using System.Runtime.CompilerServices;

namespace Spz.NET.Helpers;

public static class FixedPointHelpers
{
    /// <summary>
    /// Converts a floating-point value into a 24-bit fixed-point number with a specified amount of bits to represent the fractional portion.
    /// </summary>
    /// <param name="value">Number to convert.</param>
    /// <param name="fractionalBits">Bits dedicated to the fractional portion.</param>
    /// <returns>Fixed-point number.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fixed24 ToFixed(this float value, int fractionalBits) => new(value, fractionalBits);

    /// <summary>
    /// Converts a Vector3 value into a 24-bit fixed-point Vector with a specified amount of bits to represent the fractional portion of each component.
    /// </summary>
    /// <param name="value">Vector to convert.</param>
    /// <param name="fractionalBits">Bits dedicated to the fractional portion.</param>
    /// <returns>Fixed-point vector.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FixedVector3 ToFixed(this Vector3 value, int fractionalBits) => new(value, fractionalBits);

    /// <summary>
    /// Ensures a float is within 0-255 range to ensure safe byte casting.
    /// </summary>
    /// <param name="value">The float to clamp.</param>
    /// <returns>Safely-casted byte.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte ByteClamp(this float value) => (byte)Math.Min(Math.Max(0d, Math.Round(value)), 255d);
}
