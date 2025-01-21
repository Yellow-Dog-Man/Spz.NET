using System.Numerics;

namespace SharPZ.Tests;

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
    
    [TestMethod]
    public void TestFloatToFixed12bit()
    {
        int fractionalBits = 12;
        int floatTestLength = 1024;
        float acceptableError = 0.001f;

        float fixedMin = Fixed24.MinValue(fractionalBits);
        float fixedMax = Fixed24.MaxValue(fractionalBits);

        for (int i = 0; i < floatTestLength; i++)
        {
            float original = RandomFloat(fixedMin, fixedMax);
            Fixed24 fixed24 = original.ToFixed(fractionalBits);
            float suspect = fixed24.ToFloat(fractionalBits);

            bool withinError = Approximately(original, suspect, acceptableError);
            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {MathF.Abs(original - suspect)}");
        }
    }



    [TestMethod]
    public void TestVector3ToFixed12bit()
    {
        int fractionalBits = 12;
        int floatTestLength = 1024;
        float acceptableError = 0.001f;

        float fixedMin = Fixed24.MinValue(fractionalBits);
        float fixedMax = Fixed24.MaxValue(fractionalBits);

        for (int i = 0; i < floatTestLength; i++)
        {
            Vector3 original = new(RandomFloat(fixedMin, fixedMax), RandomFloat(fixedMin, fixedMax), RandomFloat(fixedMin, fixedMax));
            FixedVector3 fixed24 = original.ToFixed(fractionalBits);
            Vector3 suspect = fixed24.ToVector3(fractionalBits);

            bool withinError = Approximately(original, suspect, acceptableError);
            Assert.IsTrue(withinError, $"Suspect was not within an acceptable error of {acceptableError}. Original: {original}, Suspect: {suspect}, Error: {Vector3.Abs(original - suspect)}");
        }
    }



    [TestMethod]
    public void TestGaussianPacking()
    {
        int fractionalBits = 12;
        // int floatTestLength = 1024;
        // float acceptableError = 0.001f;

        Gaussian original = new(
            new(0.43915567f, 0.779961f, -6.8418803f),
            new(-4.338518f, -4.4524884f, -6.8418803f),
            new(0.7715423f, 0.779961f, 0.07984782f, 0.24494846f),
            -0.31454092f,
            new(0.6694145f, 0.7451919f, 0.5009876f),
            new(
                new(-0.005022233f, -0.007789358f, -0.0065335752f),
                new(0.004808277f, -0.0059882556f, -0.0033241967f),
                new(0.0069579175f, -0.011957922f, -0.015888745f),
                new(0.015009115f, 0.01645963f, 0.013645758f),
                new(0.010789621f, 0.013812945f, 0.01335298f),
                new(-0.005126005f, -0.010236794f, -0.0073701525f),
                new(0.032350097f, 0.030966925f, 0.023583187f),
                new(-0.011732013f, -0.007408888f, -0.0032132515f),
                new(-0.013759245f, -0.012922256f, -0.012700858f),
                new(-0.011898138f, -0.014157681f, -0.012344559f),
                new(0.021750072f, 0.025049489f, 0.025463518f),
                new(0.020837674f, 0.01876398f, 0.013896407f),
                new(0.03668385f, 0.038249563f, 0.027695632f),
                new(-0.01862f, -0.018838165f, -0.008320158f),
                new(0.028240616f, 0.026292708f, 0.020100184f),
                new(0f, 0f, 0f)
            )
        );

        PackedGaussian packed = original.Pack(fractionalBits);

        Gaussian suspect = packed.Unpack(fractionalBits);
    }
}
