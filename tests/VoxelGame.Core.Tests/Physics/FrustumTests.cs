// <copyright file="FrustumTests.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using Xunit;

namespace VoxelGame.Core.Tests.Physics;

public class FrustumTests
{
    [Fact]
    public void TestNoIntersection()
    {
        Frustum frustum = new(Math.PI / 2,
            ratio: 1,
            (1, 1000),
            Vector3d.Zero,
            Vector3d.UnitX,
            Vector3d.UnitY,
            Vector3d.UnitZ);

        // Box comes before the near plane:
        Box3d boxA = new(Vector3d.One * -0.5, Vector3d.One * 0.5);
        Assert.False(frustum.IsBoxInFrustum(boxA));

        // Box comes after the far plane:
        Box3d boxB = new((1000.5, -0.5, -0.5), (1001.5, 0.5, 0.5));
        Assert.False(frustum.IsBoxInFrustum(boxB));

        // Box is right of the near plane:
        Box3d boxC = new((-0.5, -0.5, 1.5), (0.5, 0.5, 2.5));
        Assert.False(frustum.IsBoxInFrustum(boxC));

        // Box is right of the far plane:
        Box3d boxD = new((999.5, -0.5, 1000.5), (1000.5, 0.5, 1001.5));
        Assert.False(frustum.IsBoxInFrustum(boxD));
    }

    [Fact]
    public void TestLargeFrustum()
    {
        Frustum frustum = new(Math.PI / 2,
            ratio: 1,
            (1, 1000),
            Vector3d.Zero,
            Vector3d.UnitX,
            Vector3d.UnitY,
            Vector3d.UnitZ);

        Box3d box = new((500, -1.0, -1.0), (501, 1.0, 1.0));
        Assert.True(frustum.IsBoxInFrustum(box));
    }

    [Fact]
    public void TestLargeBox()
    {
        Frustum frustum = new(Math.PI / 2,
            ratio: 1,
            (1, 10),
            Vector3d.Zero,
            Vector3d.UnitX,
            Vector3d.UnitY,
            Vector3d.UnitZ);

        Box3d box = new((-1000.0, -1000.0, -1000.0), (1000.0, 1000.0, 1000.0));
        Assert.True(frustum.IsBoxInFrustum(box));
    }

    [Fact]
    public void TestOverlap()
    {
        Frustum frustum = new(Math.PI / 2,
            ratio: 1,
            (1, 1000),
            Vector3d.Zero,
            Vector3d.UnitX,
            Vector3d.UnitY,
            Vector3d.UnitZ);

        Box3d box = new((-1.0, -1.0, -1.0), (1.0, 1.0, 1.0));
        Assert.True(frustum.IsBoxInFrustum(box));
    }
}
