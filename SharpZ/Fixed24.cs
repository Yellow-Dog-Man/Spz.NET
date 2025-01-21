using System.Runtime.CompilerServices;

namespace SharPZ;

public readonly struct Fixed24
{
    const int SIGN_BIT_MASK_24 = 0x800000;
    const uint SIGN_BIT_MASK_32 = 0xff000000;
    private readonly byte b0;
    private readonly byte b1;
    private readonly byte b2;


    public Fixed24(float value, int fractionalBits)
    {
        float scale = 1 << fractionalBits;
        int fixed32 = (int)Math.Round(value * scale);

    
        b0 = (byte)(fixed32 & 0xff);
        b1 = (byte)((fixed32 >> 8) & 0xff);
        b2 = (byte)((fixed32 >> 16) & 0xff);
    }

    public Fixed24(byte b0, byte b1, byte b2)
    {
        this.b0 = b0;
        this.b1 = b1;
        this.b2 = b2;
    }


    public readonly float ToFloat(int fractionalBits)
    {
        float scale = 1f / (1 << fractionalBits);
        uint fixed32 = b0;
        fixed32 |= (uint)b1 << 8;
        fixed32 |= (uint)b2 << 16;
        fixed32 |= (fixed32 & SIGN_BIT_MASK_24) == 0 ? 0 : SIGN_BIT_MASK_32;

        return unchecked((int)fixed32) * scale;
    }

    public static float MaxValue(int fractionalBits) => (1 << fractionalBits) / 2f;
    public static float MinValue(int fractionalBits) => -((1 << fractionalBits) / 2f);

    public override string ToString()
    {
        return $"{b0 | (b1 << 8) | (b2 << 16)}";
    }
}
