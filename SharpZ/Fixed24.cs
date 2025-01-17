namespace SharPZ;

public readonly struct Fixed24
{
    const uint FRACTIONAL_BITS_MASK = 0xff000000;
    public readonly byte FractionalBits => (byte)((_value & FRACTIONAL_BITS_MASK) >> 24);
    private readonly int _value;

    
    Fixed24(int value, byte fractionalBits)
    {
        _value = (fractionalBits << 24) + (int)(value & ~FRACTIONAL_BITS_MASK);
    }

    public static Fixed24 FromFloat(float x, byte fractionalBits)
    {
        int whole = (int)Math.Round(x * (1 << fractionalBits));


        // Console.WriteLine((int)whole >> fractionalBits);
        return new(whole, fractionalBits);
    }


    public override string ToString()
    {
        return $"{(_value & ~FRACTIONAL_BITS_MASK) >> FractionalBits}.{((((1 << FractionalBits) - 1) & _value) << FractionalBits) / (1 << FractionalBits)}";
    }
}
