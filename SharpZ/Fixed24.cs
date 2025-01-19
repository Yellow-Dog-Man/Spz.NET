namespace SharPZ;

public readonly struct Fixed24
{
    private readonly byte b0;
    private readonly byte b1;
    private readonly byte b2;


    public Fixed24(float value, int fractionalBits)
    {
        int fixed32 = (int)Math.Round(value * (1 << fractionalBits));
        if (fixed32 > 1 << 24)
            throw new IndexOutOfRangeException($"Fixed24 only supports values up to {1 << 24}, value resulted in: {value}");
        
        b0 = (byte)(fixed32 & 0xff);
        b1 = (byte)((fixed32 >> 8) & 0xff);
        b2 = (byte)((fixed32 >> 16) & 0xff);
    }


    public readonly float ToFloat(int fractionalBits)
    {
        int fixed32 = b0 | (b1 << 8) | (b2 << 16);

        int fractionalScale = 1 << fractionalBits;

        return (fixed32 >> fractionalBits) + (((fractionalScale - 1) & fixed32) / (float)fractionalScale);
    }

    public override string ToString()
    {
        return $"{b0 | (b1 << 8) | (b2 << 16)}";
    }
}
