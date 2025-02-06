using System.Numerics;
using Spz.NET.Helpers;

namespace Spz.NET.Tests;

[TestClass]
public sealed class QuantizationTests
{
    public static bool Approximately(float a, float b, float epsilon)
    {
        return MathF.Abs(a - b) <= epsilon;
    }

    public static bool Approximately(Vector3 a, Vector3 b, float epsilon)
    {
        Vector3 error = Vector3.Abs(a - b);
        return error.X <= epsilon && error.Y <= epsilon && error.Z <= epsilon;
    }

    public static float RandomFloat(float min = 0f, float max = 1f)
    {
        return min + (max - min) * Random.Shared.NextSingle();
    }



    static float ToDegrees(float radians) => radians * (180f / MathF.PI);
    static float ToRadians(float degrees) => degrees * (MathF.PI / 180f);
    static Quaternion FromEuler(float p, float y, float r) => Quaternion.CreateFromYawPitchRoll(ToRadians(y), ToRadians(p), ToRadians(r));
    static float Angle(Quaternion a, Quaternion b)
    {
        Quaternion relativeRotation = a * Quaternion.Inverse(b);

        return 2*MathF.Asin(relativeRotation.AsVector4().AsVector3().Length());
    }
    static Quaternion RandomRotation() => Quaternion.Normalize(new Quaternion(RandomFloat(-1f, 1f), RandomFloat(-1f, 1f), RandomFloat(-1f, 1f), RandomFloat(-1f, 1f)));

    
    [TestMethod]
    public void FloatToFixedTest()
    {
        int fractionalBits = 12;
        float acceptableError = 0.001f;

        float fixedMin = Fixed24.MinValue(fractionalBits);
        float fixedMax = Fixed24.MaxValue(fractionalBits);

        // Test a thousand times just to be sure. (Is there a better way to do this?)
        for (int i = 0; i < 1000; i++)
        {
            float original = RandomFloat(fixedMin, fixedMax);
            Fixed24 fixed24 = original.ToFixed(fractionalBits);
            float suspect = fixed24.ToFloat(fractionalBits);

            bool withinError = Approximately(original, suspect, acceptableError);
            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {MathF.Abs(original - suspect)}");
        }
    }



    [TestMethod]
    public void Vector3ToFixedTest()
    {
        int fractionalBits = 12;
        float acceptableError = 0.001f;

        float fixedMin = Fixed24.MinValue(fractionalBits);
        float fixedMax = Fixed24.MaxValue(fractionalBits);

        // Test a thousand times just to be sure. (Is there a better way to do this?)
        int count = 1000;
        for (int i = 0; i < count; i++)
        {
            Vector3 original = new(RandomFloat(fixedMin, fixedMax), RandomFloat(fixedMin, fixedMax), RandomFloat(fixedMin, fixedMax));
            FixedVector3 fixed24 = original.ToFixed(fractionalBits);
            Vector3 suspect = fixed24.ToVector3(fractionalBits);

            bool withinError = Approximately(original, suspect, acceptableError);
            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {Vector3.Abs(original - suspect)}");
        }
    }



    [TestMethod]
    public void QuaternionQuantizationTest()
    {
        float acceptableError = 18f; // 8 bits isn't a lot of precision, so hefty margins are required.

        // Test a thousand times just to be sure. (Is there a better way to do this?)
        int count = 1000;
        for (int i = 0; i < count; i++)
        {
            Quaternion original = RandomRotation();
            QuantizedQuat quantized = original;
            Quaternion suspect = quantized;
            float angle = ToDegrees(Angle(original, suspect));

            bool withinError = angle < acceptableError;

            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {angle}");
        }
    }



    [TestMethod]
    public void ScaleQuantizationTest()
    {
        float acceptableError = 0.06f;

        // Count up in very small increments to test the error.
        int count = 1000;
        for (int i = 0; i < count; i++)
        {
            Vector3 original = new((float)i / count, (float)i / count, (float)i / count);
            QuantizedScale quantized = original;
            Vector3 suspect = quantized;

            Vector3 error = original - suspect;

            float maxError = MathF.Max(MathF.Max(error.X, error.Y), error.Z);

            bool withinError = Approximately(original, suspect, acceptableError);

            // Console.WriteLine($"Suspect: {suspect}, Original: {original}, Error: {original.Length() - suspect.Length()}");

            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {maxError}");
        }
    }



    [TestMethod]
    public void ColorQuantizationTest()
    {
        float acceptableError = 0.1f;

        // Count up in very small increments to test the error.
        int count = 1000;
        for (int i = 0; i < count; i++)
        {
            // Quantized colors can go up to 3.333 repeating for when spherical harmonics bring the colors back down to SDR.
            Vector3 original = new Vector3((float)i / count, (float)i / count, (float)i / count) * (3f + 1f / 3f);
            QuantizedColor quantized = original;
            Vector3 suspect = quantized;

            Vector3 error = original - suspect;

            float maxError = MathF.Max(MathF.Max(error.X, error.Y), error.Z);

            bool withinError = Approximately(original, suspect, acceptableError);

            // Console.WriteLine($"Suspect: {suspect}, Original: {original}, Error: {original.Length() - suspect.Length()}");

            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {maxError}");
        }
    }



    [TestMethod]
    public void AlphaQuantizationTest()
    {
        // Alpha is quantized with sigmoid activation, so the upper regions can be up to ~0.65f off.
        float acceptableError = 0.65f; // TODO: Make corresponding sigmoid function to test errors at smaller ranges?

        // Count up in very small increments to test the error.
        int count = 1000;
        for (int i = 0; i < count; i++)
        {
            float original = ((float)i / count) * 12f - 6f;
            QuantizedAlpha quantized = original;
            float suspect = quantized;

            float error = original - suspect;

            bool withinError = Approximately(original, suspect, acceptableError);

            // Console.WriteLine($"Suspect: {suspect}, Original: {original}, Error: {original - suspect}");

            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {error}");
        }
    }



    [TestMethod]
    public void HarmonicQuantizationTest()
    {
        float acceptableError5bit = 0.036f; // First 9 components of a harmonic are encoded 5 bits for -1 to 1.
        float acceptableErrorRest = 0.066f; // Rest of the components are encoded 4 bits for -1 to 1, so a slightler bigger error is acceptable.

        Span<byte> quantized = stackalloc byte[45];

        // Count up in very small increments to test the error.
        int count = 1000;
        for (int i = 0; i < count; i++)
        {
            GaussianHarmonics original = new();
            int j = 45;
            while (j-- > 0)
                original[j] = ((float)i / count) * 2f + -1f;

            original.Quantize(quantized);

            quantized.Unquantize(out GaussianHarmonics suspect);

            float maxError5bit = 0f;
            for (j = 0; j < 9; j++)
            {
                float error = original[j] - suspect[j];
                maxError5bit = MathF.Max(maxError5bit, MathF.Abs(error));
            }

            float maxErrorRest = 0f;
            for (j = 9; j < 45; j++)
            {
                float error = original[j] - suspect[j];
                maxErrorRest = MathF.Max(maxErrorRest, MathF.Abs(error));
            }


            if (maxError5bit > acceptableError5bit)
                Assert.Fail($"Suspect was not within an acceptable error of {acceptableError5bit}. Original: {original}, Suspect: {suspect}, Error: {maxError5bit}");
            
            if (maxErrorRest > acceptableErrorRest)
                Assert.Fail($"Suspect was not within an acceptable error of {acceptableErrorRest}. Original: {original}, Suspect: {suspect}, Error: {maxErrorRest}");
        }
    }
}
