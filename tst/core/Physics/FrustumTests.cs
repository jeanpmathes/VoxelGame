// <copyright file="FrustumTests.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// </copyright>
// <author>jeanpmathes</author>

using System;
using JetBrains.Annotations;
using OpenTK.Mathematics;
using VoxelGame.Core.Physics;
using Xunit;

namespace VoxelGame.Core.Tests.Physics;

[TestSubject(typeof(Frustum))]
public class FrustumTests
{
    [Fact]
    public void Frustum_IsBoxInFrustum_ShouldNotContainDistantBoxes()
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
    public void Frustum_IsBoxInFrustum_ShouldContainSmallBoxEvenIfLarge()
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
    public void Frustum_IsBoxInFrustum_ShouldContainLargeBoxEvenIfSmall()
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
    public void Frustum_IsBoxInFrustum_ShouldContainOverlappingBox()
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
