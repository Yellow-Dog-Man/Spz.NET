using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Spz.NET;


/// <summary>
/// Represents the spherical harmonics of a given gaussian.
/// </summary>
/// <typeparam name="T">The type to use for each coefficient's components.</typeparam>
public struct GaussianHarmonics : IEquatable<GaussianHarmonics>
{
    public const int COEFFICIENT_LENGTH = 15;
    public const int COEFFICIENT_COMPONENTS = 45;

    public readonly unsafe float this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => GetElement(in this, index % 45);

        // index switch
        // {
        //     0 => Component0,
        //     1 => Component1,
        //     2 => Component2,
        //     3 => Component3,
        //     4 => Component4,
        //     5 => Component5,
        //     6 => Component6,
        //     7 => Component7,
        //     8 => Component8,
        //     9 => Component9,
        //     10 => Component10,
        //     11 => Component11,
        //     12 => Component12,
        //     13 => Component13,
        //     14 => Component14,
        //     15 => Component15,
        //     16 => Component16,
        //     17 => Component17,
        //     18 => Component18,
        //     19 => Component19,
        //     20 => Component20,
        //     21 => Component21,
        //     22 => Component22,
        //     23 => Component23,
        //     24 => Component24,
        //     25 => Component25,
        //     26 => Component26,
        //     27 => Component27,
        //     28 => Component28,
        //     29 => Component29,
        //     30 => Component30,
        //     31 => Component31,
        //     32 => Component32,
        //     33 => Component33,
        //     34 => Component34,
        //     35 => Component35,
        //     36 => Component36,
        //     37 => Component37,
        //     38 => Component38,
        //     39 => Component39,
        //     40 => Component40,
        //     41 => Component41,
        //     42 => Component42,
        //     43 => Component43,
        //     44 => Component44,
        //     // _ => throw new IndexOutOfRangeException()
        // };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => GetElement(in this, index % 45) = value;
        // {
            // switch (index)
            // {
            //     case 0:
            //         Component0 = value;
            //         break;
            //     case 1:
            //         Component1 = value;
            //         break;
            //     case 2:
            //         Component2 = value;
            //         break;
            //     case 3:
            //         Component3 = value;
            //         break;
            //     case 4:
            //         Component4 = value;
            //         break;
            //     case 5:
            //         Component5 = value;
            //         break;
            //     case 6:
            //         Component6 = value;
            //         break;
            //     case 7:
            //         Component7 = value;
            //         break;
            //     case 8:
            //         Component8 = value;
            //         break;
            //     case 9:
            //         Component9 = value;
            //         break;
            //     case 10:
            //         Component10 = value;
            //         break;
            //     case 11:
            //         Component11 = value;
            //         break;
            //     case 12:
            //         Component12 = value;
            //         break;
            //     case 13:
            //         Component13 = value;
            //         break;
            //     case 14:
            //         Component14 = value;
            //         break;
            //     case 15:
            //         Component15 = value;
            //         break;
            //     case 16:
            //         Component16 = value;
            //         break;
            //     case 17:
            //         Component17 = value;
            //         break;
            //     case 18:
            //         Component18 = value;
            //         break;
            //     case 19:
            //         Component19 = value;
            //         break;
            //     case 20:
            //         Component20 = value;
            //         break;
            //     case 21:
            //         Component21 = value;
            //         break;
            //     case 22:
            //         Component22 = value;
            //         break;
            //     case 23:
            //         Component23 = value;
            //         break;
            //     case 24:
            //         Component24 = value;
            //         break;
            //     case 25:
            //         Component25 = value;
            //         break;
            //     case 26:
            //         Component26 = value;
            //         break;
            //     case 27:
            //         Component27 = value;
            //         break;
            //     case 28:
            //         Component28 = value;
            //         break;
            //     case 29:
            //         Component29 = value;
            //         break;
            //     case 30:
            //         Component30 = value;
            //         break;
            //     case 31:
            //         Component31 = value;
            //         break;
            //     case 32:
            //         Component32 = value;
            //         break;
            //     case 33:
            //         Component33 = value;
            //         break;
            //     case 34:
            //         Component34 = value;
            //         break;
            //     case 35:
            //         Component35 = value;
            //         break;
            //     case 36:
            //         Component36 = value;
            //         break;
            //     case 37:
            //         Component37 = value;
            //         break;
            //     case 38:
            //         Component38 = value;
            //         break;
            //     case 39:
            //         Component39 = value;
            //         break;
            //     case 40:
            //         Component40 = value;
            //         break;
            //     case 41:
            //         Component41 = value;
            //         break;
            //     case 42:
            //         Component42 = value;
            //         break;
            //     case 43:
            //         Component43 = value;
            //         break;
            //     case 44:
            //         Component44 = value;
            //         break;
            //     default:
            //         throw new IndexOutOfRangeException();
            // }
        // }
    }

    public float Component0;
    public float Component1;
    public float Component2;
    public float Component3;
    public float Component4;
    public float Component5;
    public float Component6;
    public float Component7;
    public float Component8;
    public float Component9;
    public float Component10;
    public float Component11;
    public float Component12;
    public float Component13;
    public float Component14;
    public float Component15;
    public float Component16;
    public float Component17;
    public float Component18;
    public float Component19;
    public float Component20;
    public float Component21;
    public float Component22;
    public float Component23;
    public float Component24;
    public float Component25;
    public float Component26;
    public float Component27;
    public float Component28;
    public float Component29;
    public float Component30;
    public float Component31;
    public float Component32;
    public float Component33;
    public float Component34;
    public float Component35;
    public float Component36;
    public float Component37;
    public float Component38;
    public float Component39;
    public float Component40;
    public float Component41;
    public float Component42;
    public float Component43;
    public float Component44;


    public unsafe GaussianHarmonics(
        float component0,
        float component1,
        float component2,
        float component3,
        float component4,
        float component5,
        float component6,
        float component7,
        float component8,
        float component9,
        float component10,
        float component11,
        float component12,
        float component13,
        float component14,
        float component15,
        float component16,
        float component17,
        float component18,
        float component19,
        float component20,
        float component21,
        float component22,
        float component23,
        float component24,
        float component25,
        float component26,
        float component27,
        float component28,
        float component29,
        float component30,
        float component31,
        float component32,
        float component33,
        float component34,
        float component35,
        float component36,
        float component37,
        float component38,
        float component39,
        float component40,
        float component41,
        float component42,
        float component43,
        float component44
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


    /// <summary>
    /// Transposes the harmonics such that each row of components corresponds to a given color channel. Ex:
    /// <para>
    /// [RRR]
    /// </para>
    /// <para>
    /// [GGG]
    /// </para>
    /// <para>
    /// [BBB]
    /// </para>
    /// </summary>
    /// <returns>The transposed harmonics.</returns>
    public readonly GaussianHarmonics ToNCS()
    {
        return new(
            Component0,  Component3,  Component6,  Component9,  Component12, Component15, Component18, Component21, Component24, Component27, Component30, Component33, Component36, Component39, Component42,
            Component1,  Component4,  Component7,  Component10, Component13, Component16, Component19, Component22, Component25, Component28, Component31, Component34, Component37, Component40, Component43,
            Component2,  Component5,  Component8,  Component11, Component14, Component17, Component20, Component23, Component26, Component29, Component32, Component35, Component38, Component41, Component44);
    }


    /// <summary>
    /// Transposes the harmonics such that each row of components corresponds to each color.
    /// <para>
    /// [RGB]
    /// </para>
    /// <para>
    /// [RGB]
    /// </para>
    /// <para>
    /// [RGB]
    /// </para>
    /// </summary>
    /// <returns>The transposed harmonics.</returns>
    public readonly GaussianHarmonics ToNSC()
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

    public readonly bool Equals(GaussianHarmonics other)
    {
        bool condition = true;
        int i = COEFFICIENT_COMPONENTS;
        while (i-- > 0)
            condition &= this[i].Equals(other[i]);

        return condition;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref float GetElement(in GaussianHarmonics harmonics, int index)
    {
        return ref Unsafe.Add(ref GetFirst(in harmonics), index);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ref float GetFirst(in GaussianHarmonics harmonics)
    {
        return ref Unsafe.AsRef(in harmonics.Component0);
    }


    public static bool operator ==(in GaussianHarmonics left, in GaussianHarmonics right) => left.Equals(right);

    public static bool operator !=(in GaussianHarmonics left, in GaussianHarmonics right) => !(left == right);
}