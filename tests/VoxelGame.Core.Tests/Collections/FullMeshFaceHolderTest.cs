// <copyright file="FullMeshFaceHolderTest.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using Xunit;

namespace VoxelGame.Core.Tests.Collections;

public class FullMeshFaceHolderTest
{
    private static readonly (uint, uint, uint, uint) emptyData = (0, 0, 0, 0);

    private static void FillHolder(FullMeshFaceHolder holder, Vector3i direction)
    {
        bool IsSkippedIndex(int index, int component)
        {
            return component switch
            {
                -1 => index != 0,
                1 => index != Section.Size - 1,
                _ => false
            };
        }

        for (var x = 0; x < Section.Size; x++)
        for (var y = 0; y < Section.Size; y++)
        for (var z = 0; z < Section.Size; z++)
        {
            if (IsSkippedIndex(x, direction.X) || IsSkippedIndex(y, direction.Y) || IsSkippedIndex(z, direction.Z))
                continue;

            holder.AddFace((x, y, z), emptyData, isRotated: false);
        }
    }

    private static Vector3[] GetSideMesh(BlockSide side)
    {
        side.Corners(out int[] a, out int[] b, out int[] c, out int[] d);

        Vector3 GetVector(IReadOnlyList<int> arr)
        {
            return new Vector3(arr[index: 0], arr[index: 1], arr[index: 2]) * Section.Size;
        }

        return new[]
        {
            GetVector(a),
            GetVector(b),
            GetVector(c),
            GetVector(d)
        };
    }

    private static void TestSide(BlockSide side)
    {
        FullMeshFaceHolder holder = new(side);

        FillHolder(holder, side.Direction());

        PooledList<SpatialVertex> vertices = new();

        holder.GenerateMesh(vertices);

        Assert.Equal(expected: 4, vertices.Count);

        Vector3[] expected = GetSideMesh(side);

        Assert.True(VMath.NearlyEqual(expected[0], vertices[index: 0].Position));
        Assert.True(VMath.NearlyEqual(expected[1], vertices[index: 1].Position));
        Assert.True(VMath.NearlyEqual(expected[2], vertices[index: 2].Position));
        Assert.True(VMath.NearlyEqual(expected[3], vertices[index: 3].Position));

        vertices.ReturnToPool();
    }

    [Fact]
    public void TestFrontSide()
    {
        TestSide(BlockSide.Front);
    }

    [Fact]
    public void TestBackSide()
    {
        TestSide(BlockSide.Back);
    }

    [Fact]
    public void TestLeftSide()
    {
        TestSide(BlockSide.Left);
    }

    [Fact]
    public void TestRightSide()
    {
        TestSide(BlockSide.Right);
    }

    [Fact]
    public void TestBottomSide()
    {
        TestSide(BlockSide.Bottom);
    }

    [Fact]
    public void TestTopSide()
    {
        TestSide(BlockSide.Top);
    }
}
