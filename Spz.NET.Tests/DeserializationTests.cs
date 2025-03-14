﻿using System.Numerics;
using Spz.NET.Helpers;

namespace Spz.NET.Tests;

[TestClass]
public sealed class DeserializationTests
{
    [TestMethod]
    public void DeserializeSpzTest()
    {
        var cloud = Spz.NET.Serialization.SplatSerializer.FromSpz(@"hornedlizard.spz");

        Assert.IsNotNull(cloud);
        Assert.AreNotEqual(0, cloud.Count, 0);

        for(int i = 0; i < cloud.Count; i++)
        {
            var splat = cloud[i];

            Assert.AreNotEqual(Vector3.Zero, splat.Position, $"Zero position at index {i}");
            Assert.AreNotEqual(Quaternion.Zero, splat.Rotation, $"Zero rotation at index {i}");
            Assert.AreNotEqual(Vector3.Zero, splat.Scale, $"Zero scale at index {i}");
        }
    }
}