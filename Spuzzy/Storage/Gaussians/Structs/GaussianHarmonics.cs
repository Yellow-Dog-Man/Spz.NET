using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Spuzzy;


public struct GaussianHarmonics<T>
    where T : unmanaged
{
    public const int COEFFICIENT_LENGTH = 15;
    public const int COEFFICIENT_COMPONENTS = 45;

    public unsafe T this[int index]
    {
        readonly get => index switch
        {
            0 => Component0,
            1 => Component1,
            2 => Component2,
            3 => Component3,
            4 => Component4,
            5 => Component5,
            6 => Component6,
            7 => Component7,
            8 => Component8,
            9 => Component9,
            10 => Component10,
            11 => Component11,
            12 => Component12,
            13 => Component13,
            14 => Component14,
            15 => Component15,
            16 => Component16,
            17 => Component17,
            18 => Component18,
            19 => Component19,
            20 => Component20,
            21 => Component21,
            22 => Component22,
            23 => Component23,
            24 => Component24,
            25 => Component25,
            26 => Component26,
            27 => Component27,
            28 => Component28,
            29 => Component29,
            30 => Component30,
            31 => Component31,
            32 => Component32,
            33 => Component33,
            34 => Component34,
            35 => Component35,
            36 => Component36,
            37 => Component37,
            38 => Component38,
            39 => Component39,
            40 => Component40,
            41 => Component41,
            42 => Component42,
            43 => Component43,
            44 => Component44,
            _ => throw new IndexOutOfRangeException()
        };
    
        set
        {
            switch (index)
            {
                case 0:
                    Component0 = value;
                    break;
                case 1:
                    Component1 = value;
                    break;
                case 2:
                    Component2 = value;
                    break;
                case 3:
                    Component3 = value;
                    break;
                case 4:
                    Component4 = value;
                    break;
                case 5:
                    Component5 = value;
                    break;
                case 6:
                    Component6 = value;
                    break;
                case 7:
                    Component7 = value;
                    break;
                case 8:
                    Component8 = value;
                    break;
                case 9:
                    Component9 = value;
                    break;
                case 10:
                    Component10 = value;
                    break;
                case 11:
                    Component11 = value;
                    break;
                case 12:
                    Component12 = value;
                    break;
                case 13:
                    Component13 = value;
                    break;
                case 14:
                    Component14 = value;
                    break;
                case 15:
                    Component15 = value;
                    break;
                case 16:
                    Component16 = value;
                    break;
                case 17:
                    Component17 = value;
                    break;
                case 18:
                    Component18 = value;
                    break;
                case 19:
                    Component19 = value;
                    break;
                case 20:
                    Component20 = value;
                    break;
                case 21:
                    Component21 = value;
                    break;
                case 22:
                    Component22 = value;
                    break;
                case 23:
                    Component23 = value;
                    break;
                case 24:
                    Component24 = value;
                    break;
                case 25:
                    Component25 = value;
                    break;
                case 26:
                    Component26 = value;
                    break;
                case 27:
                    Component27 = value;
                    break;
                case 28:
                    Component28 = value;
                    break;
                case 29:
                    Component29 = value;
                    break;
                case 30:
                    Component30 = value;
                    break;
                case 31:
                    Component31 = value;
                    break;
                case 32:
                    Component32 = value;
                    break;
                case 33:
                    Component33 = value;
                    break;
                case 34:
                    Component34 = value;
                    break;
                case 35:
                    Component35 = value;
                    break;
                case 36:
                    Component36 = value;
                    break;
                case 37:
                    Component37 = value;
                    break;
                case 38:
                    Component38 = value;
                    break;
                case 39:
                    Component39 = value;
                    break;
                case 40:
                    Component40 = value;
                    break;
                case 41:
                    Component41 = value;
                    break;
                case 42:
                    Component42 = value;
                    break;
                case 43:
                    Component43 = value;
                    break;
                case 44:
                    Component44 = value;
                    break;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }

    public T Component0;
    public T Component1;
    public T Component2;
    public T Component3;
    public T Component4;
    public T Component5;
    public T Component6;
    public T Component7;
    public T Component8;
    public T Component9;
    public T Component10;
    public T Component11;
    public T Component12;
    public T Component13;
    public T Component14;
    public T Component15;
    public T Component16;
    public T Component17;
    public T Component18;
    public T Component19;
    public T Component20;
    public T Component21;
    public T Component22;
    public T Component23;
    public T Component24;
    public T Component25;
    public T Component26;
    public T Component27;
    public T Component28;
    public T Component29;
    public T Component30;
    public T Component31;
    public T Component32;
    public T Component33;
    public T Component34;
    public T Component35;
    public T Component36;
    public T Component37;
    public T Component38;
    public T Component39;
    public T Component40;
    public T Component41;
    public T Component42;
    public T Component43;
    public T Component44;


    public GaussianHarmonics(Span<T> data)
    {
        int i = data.Length;
        while (i-- > 0)
            this[i] = data[i];
    }


    public unsafe GaussianHarmonics(
        T component0,
        T component1,
        T component2,
        T component3,
        T component4,
        T component5,
        T component6,
        T component7,
        T component8,
        T component9,
        T component10,
        T component11,
        T component12,
        T component13,
        T component14,
        T component15,
        T component16,
        T component17,
        T component18,
        T component19,
        T component20,
        T component21,
        T component22,
        T component23,
        T component24,
        T component25,
        T component26,
        T component27,
        T component28,
        T component29,
        T component30,
        T component31,
        T component32,
        T component33,
        T component34,
        T component35,
        T component36,
        T component37,
        T component38,
        T component39,
        T component40,
        T component41,
        T component42,
        T component43,
        T component44
    )
    {
        Component0 = component0;
        Component1 = component1;
        Component2 = component2;
        Component3 = component3;
        Component4 = component4;
        Component5 = component5;
        Component6 = component6;
        Component7 = component7;
        Component8 = component8;
        Component9 = component9;
        Component10 = component10;
        Component11 = component11;
        Component12 = component12;
        Component13 = component13;
        Component14 = component14;
        Component15 = component15;
        Component16 = component16;
        Component17 = component17;
        Component18 = component18;
        Component19 = component19;
        Component20 = component20;
        Component21 = component21;
        Component22 = component22;
        Component23 = component23;
        Component24 = component24;
        Component25 = component25;
        Component26 = component26;
        Component27 = component27;
        Component28 = component28;
        Component29 = component29;
        Component30 = component30;
        Component31 = component31;
        Component32 = component32;
        Component33 = component33;
        Component34 = component34;
        Component35 = component35;
        Component36 = component36;
        Component37 = component37;
        Component38 = component38;
        Component39 = component39;
        Component40 = component40;
        Component41 = component41;
        Component42 = component42;
        Component43 = component43;
        Component44 = component44;
    }


    public readonly GaussianHarmonics<T> ToNCS()
    {
        return new(
            Component0,  Component3,  Component6,  Component9,  Component12, Component15, Component18, Component21, Component24, Component27, Component30, Component33, Component36, Component39, Component42,
            Component1,  Component4,  Component7,  Component10, Component13, Component16, Component19, Component22, Component25, Component28, Component31, Component34, Component37, Component40, Component43,
            Component2,  Component5,  Component8,  Component11, Component14, Component17, Component20, Component23, Component26, Component29, Component32, Component35, Component38, Component41, Component44);
    }


    public readonly GaussianHarmonics<T> ToNSC()
    {
        return new(
            Component0,  Component15, Component30,
            Component1,  Component16, Component31,
            Component2,  Component17, Component32,
            Component3,  Component18, Component33,
            Component4,  Component19, Component34,
            Component5,  Component20, Component35,
            Component6,  Component21, Component36,
            Component7,  Component22, Component37,
            Component8,  Component23, Component38,
            Component9,  Component24, Component39,
            Component10, Component25, Component40,
            Component11, Component26, Component41,
            Component12, Component27, Component42,
            Component13, Component28, Component43,
            Component14, Component29, Component44
        );
    }
}