using System.Numerics;

namespace SharPZ;

public struct GaussianHarmonics(
    Vector3 c0,
    Vector3 c1,
    Vector3 c2,
    Vector3 c3,
    Vector3 c4,
    Vector3 c5,
    Vector3 c6,
    Vector3 c7,
    Vector3 c8,
    Vector3 c9,
    Vector3 c10,
    Vector3 c11,
    Vector3 c12,
    Vector3 c13,
    Vector3 c14,
    Vector3 c15)
{

    public Vector3 this[int index]
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

    public Vector3 Coefficient0  = c0;
    public Vector3 Coefficient1  = c1;
    public Vector3 Coefficient2  = c2;
    public Vector3 Coefficient3  = c3;
    public Vector3 Coefficient4  = c4;
    public Vector3 Coefficient5  = c5;
    public Vector3 Coefficient6  = c6;
    public Vector3 Coefficient7  = c7;
    public Vector3 Coefficient8  = c8;
    public Vector3 Coefficient9  = c9;
    public Vector3 Coefficient10 = c10;
    public Vector3 Coefficient11 = c11;
    public Vector3 Coefficient12 = c12;
    public Vector3 Coefficient13 = c13;
    public Vector3 Coefficient14 = c14;
    public Vector3 Coefficient15 = c15;
}