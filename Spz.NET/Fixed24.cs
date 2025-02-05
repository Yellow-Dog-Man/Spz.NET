using System.Runtime.CompilerServices;

namespace Spz.NET;


/// <summary>
/// A 24-bit fixed-point number with a given amount of fractional bits.
/// </summary>
public readonly struct Fixed24
{
    const int SIGN_BIT_MASK_24 = 0x800000;
    const uint SIGN_BIT_MASK_32 = 0xff000000;
    private readonly byte b0;
    private readonly byte b1;
    private readonly byte b2;


    /// <summary>
    /// Creates a 24-bit fixed-point number from a floating-point value.
    /// </summary>
    /// <param name="value">The number to be converted to a fixed representation.</param>
    /// <param name="fractionalBits">The number of bits dedicated to representing the fractional portion.</param>
    public Fixed24(float value, int fractionalBits)
    {
        float scale = 1 << fractionalBits;
        int fixed32 = (int)Math.Round(value * scale);

    
        b0 = (byte)(fixed32 & 0xff);
        b1 = (byte)((fixed32 >> 8) & 0xff);
        b2 = (byte)((fixed32 >> 16) & 0xff);
    }

    /// <summary>
    /// Creates a 24-bit fixed-point number from 3 bytes.
    /// </summary>
    /// <param name="b0">Byte 1.</param>
    /// <param name="b1">Byte 2.</param>
    /// <param name="b2">Byte 3.</param>
    public Fixed24(byte b0, byte b1, byte b2)
    {
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
    }


    /// <summary>
    /// Converts this fixed-point number to a floating point representation with a given number of fractional bits.
    /// </summary>
    /// <param name="fractionalBits">The number of bits dedicated to representing the fractional portion.</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly float ToFloat(int fractionalBits)
    {
        float scale = 1f / (1 << fractionalBits);
        uint fixed32 = b0;
        fixed32 |= (uint)b1 << 8;
        fixed32 |= (uint)b2 << 16;
        fixed32 |= (fixed32 & SIGN_BIT_MASK_24) == 0 ? 0 : SIGN_BIT_MASK_32;

        return unchecked((int)fixed32) * scale;
    }

    /// <summary>
    /// The maximum value that this fixed-point number can represent with a given amount of fractional bits.
    /// </summary>
    /// <param name="fractionalBits">The number of bits dedicated to representing the fractional portion.</param>
    /// <returns>A floating-point number representing the maximum value that can be represented.</returns>
    public static float MaxValue(int fractionalBits) => (1 << fractionalBits) / 2f;


    /// <summary>
    /// The minimum value that this fixed-point number can represent with a given amount of fractional bits.
    /// </summary>
    /// <param name="fractionalBits">The number of bits dedicated to representing the fractional portion.</param>
    /// <returns>A floating-point number representing the minimum value that can be represented.</returns>
    public static float MinValue(int fractionalBits) => -((1 << fractionalBits) / 2f);

    public override string ToString()
    {
        return $"{b0 | (b1 << 8) | (b2 << 16)}";
    }
}
