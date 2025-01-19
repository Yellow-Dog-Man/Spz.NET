using System.Numerics;

namespace SharPZ;

public struct QuantizedHarmonics
{

    const int SH1_BITS = 5;
    const int SHREST_BITS = 4;

    public readonly GaussianHarmonics Harmonics => new(
        Coefficient0,
        Coefficient1,
        Coefficient2,
        Coefficient3,
        Coefficient4,
        Coefficient5,
        Coefficient6,
        Coefficient7,
        Coefficient8,
        Coefficient9,
        Coefficient10,
        Coefficient11,
        Coefficient12,
        Coefficient13,
        Coefficient14,
        Coefficient15
    );

    public QuantizedHarmonics(GaussianHarmonics harmonics)
    {
        Coefficient0  = harmonics.Coefficient0.QuantizeCoefficient(SH1_BITS);
        Coefficient1  = harmonics.Coefficient1.QuantizeCoefficient(SH1_BITS);
        Coefficient2  = harmonics.Coefficient2.QuantizeCoefficient(SH1_BITS);
        Coefficient3  = harmonics.Coefficient3.QuantizeCoefficient(SHREST_BITS);
        Coefficient4  = harmonics.Coefficient4.QuantizeCoefficient(SHREST_BITS);
        Coefficient5  = harmonics.Coefficient5.QuantizeCoefficient(SHREST_BITS);
        Coefficient6  = harmonics.Coefficient6.QuantizeCoefficient(SHREST_BITS);
        Coefficient7  = harmonics.Coefficient7.QuantizeCoefficient(SHREST_BITS);
        Coefficient8  = harmonics.Coefficient8.QuantizeCoefficient(SHREST_BITS);
        Coefficient9  = harmonics.Coefficient9.QuantizeCoefficient(SHREST_BITS);
        Coefficient10 = harmonics.Coefficient10.QuantizeCoefficient(SHREST_BITS);
        Coefficient11 = harmonics.Coefficient11.QuantizeCoefficient(SHREST_BITS);
        Coefficient12 = harmonics.Coefficient12.QuantizeCoefficient(SHREST_BITS);
        Coefficient13 = harmonics.Coefficient13.QuantizeCoefficient(SHREST_BITS);
        Coefficient14 = harmonics.Coefficient14.QuantizeCoefficient(SHREST_BITS);
        Coefficient15 = harmonics.Coefficient15.QuantizeCoefficient(SHREST_BITS);
    }


    public PackedCoefficient this[int index]
    {
        readonly get => index switch
        {
            0  => Coefficient0,
            1  => Coefficient1,
            2  => Coefficient2,
            3  => Coefficient3,
            4  => Coefficient4,
            5  => Coefficient5,
            6  => Coefficient6,
            7  => Coefficient7,
            8  => Coefficient8,
            9  => Coefficient9,
            10 => Coefficient10,
            11 => Coefficient11,
            12 => Coefficient12,
            13 => Coefficient13,
            14 => Coefficient14,
            15 => Coefficient15,
            _  => throw new NotSupportedException($"Spherical harmonic degree not supported: {index}")
        };
        set
        {
            switch (index)
            {
                case 0:
                    Coefficient0 = value;
                    break;

                case 1:
                    Coefficient1 = value;
                    break;

                case 2:
                    Coefficient2 = value;
                    break;

                case 3:
                    Coefficient3 = value;
                    break;

                case 4:
                    Coefficient4 = value;
                    break;

                case 5:
                    Coefficient5 = value;
                    break;

                case 6:
                    Coefficient6 = value;
                    break;

                case 7:
                    Coefficient7 = value;
                    break;

                case 8:
                    Coefficient8 = value;
                    break;

                case 9:
                    Coefficient9 = value;
                    break;

                case 10:
                    Coefficient10 = value;
                    break;

                case 11:
                    Coefficient11 = value;
                    break;

                case 12:
                    Coefficient12 = value;
                    break;

                case 13:
                    Coefficient13 = value;
                    break;

                case 14:
                    Coefficient14 = value;
                    break;

                case 15:
                    Coefficient15 = value;
                    break;
                
                default:
                    throw new NotSupportedException($"Cannot set spherical harmonic degree: {index}");

            }
        }
    }

    public PackedCoefficient Coefficient0;
    public PackedCoefficient Coefficient1;
    public PackedCoefficient Coefficient2;
    public PackedCoefficient Coefficient3;
    public PackedCoefficient Coefficient4;
    public PackedCoefficient Coefficient5;
    public PackedCoefficient Coefficient6;
    public PackedCoefficient Coefficient7;
    public PackedCoefficient Coefficient8;
    public PackedCoefficient Coefficient9;
    public PackedCoefficient Coefficient10;
    public PackedCoefficient Coefficient11;
    public PackedCoefficient Coefficient12;
    public PackedCoefficient Coefficient13;
    public PackedCoefficient Coefficient14;
    public PackedCoefficient Coefficient15;



    public static implicit operator GaussianHarmonics(QuantizedHarmonics other) => other.Harmonics;
    public static implicit operator QuantizedHarmonics(GaussianHarmonics other) => new(other);
}



public readonly struct PackedCoefficient(byte b1, byte b2, byte b3)
{
    public readonly Vector3 Coefficient => this.UnquantizeCoefficient();

    public readonly byte X = b1;
    public readonly byte Y = b2;
    public readonly byte Z = b3;


    public static implicit operator Vector3(PackedCoefficient other) => other.Coefficient;
}