// <copyright file="TargetingBoxEffect.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Voxels;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     A rendering effect for visualizing targeting boxes in the world.
/// </summary>
public sealed class TargetingBoxEffect : IDisposable
{
    private const Single Ratio = 0.02f;

    private readonly Effect effect;
    private readonly TargetingBoxPipeline pipeline;

    private BoxCollider? currentBox;

    internal TargetingBoxEffect(Effect effect, TargetingBoxPipeline pipeline)
    {
        this.effect = effect;
        this.pipeline = pipeline;
    }

    /// <summary>
    ///     Get or set whether the effect is enabled.
    /// </summary>
    public Boolean IsEnabled
    {
        get => effect.IsEnabled;
        set => effect.IsEnabled = value;
    }

    /// <summary>
    ///     Set the information about the target required to display the box.
    /// </summary>
    /// <param name="boxCollider">The box collider to display.</param>
    /// <param name="targetedColor">The color of the target, will decide the color of the targeting box.</param>
    public void SetTarget(BoxCollider boxCollider, ColorS targetedColor)
    {
        currentBox = boxCollider;
        effect.Position = boxCollider.Position;

        using PooledList<EffectVertex> vertices = new(boxCollider.Volume.NumberOfBoxes * 6 * 4 * 6);
        BuildMeshData(boxCollider.Volume, vertices, targetedColor.Luminance < 0.5f);

        effect.SetNewVertices(vertices.AsSpan());
    }

    private static void BuildMeshData(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices, Boolean isDarkBackground)
    {
        BuildMeshDataForTopLevelBox(boundingVolume, vertices, isDarkBackground);

        if (boundingVolume.ChildCount == 0) return;

        for (var i = 0; i < boundingVolume.ChildCount; i++)
            BuildMeshData(boundingVolume[i], vertices, isDarkBackground);
    }

    private static void BuildMeshDataForTopLevelBox(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices, Boolean isDarkBackground)
    {
        var min = (Vector3) boundingVolume.Min;
        var max = (Vector3) boundingVolume.Max;

        foreach (Side side in Side.All.Sides())
            AddSide(vertices, side, min, max, isDarkBackground);
    }

    private static void AddSide(PooledList<EffectVertex> vertices, Side side, Vector3 min, Vector3 max, Boolean isDarkBackground)
    {
        side.Corners(out Int32[] c0, out Int32[] c1, out Int32[] c2, out Int32[] c3);

        Vector3 v0 = GetCorner(min, max, c0);
        Vector3 v1 = GetCorner(min, max, c1);
        Vector3 v2 = GetCorner(min, max, c2);
        Vector3 v3 = GetCorner(min, max, c3);

        const Single width = Ratio / 2.0f * -1.0f;

        Vector3 o0 = (GetCorner(-Vector3.One, Vector3.One, c0) - side.Direction()) * width;
        Vector3 o1 = (GetCorner(-Vector3.One, Vector3.One, c1) - side.Direction()) * width;
        Vector3 o2 = (GetCorner(-Vector3.One, Vector3.One, c2) - side.Direction()) * width;
        Vector3 o3 = (GetCorner(-Vector3.One, Vector3.One, c3) - side.Direction()) * width;

        AddQuad(vertices, v0, v1, v1 + o1, v0 + o0, isDarkBackground);
        AddQuad(vertices, v1, v2, v2 + o2, v1 + o1, isDarkBackground);
        AddQuad(vertices, v2, v3, v3 + o3, v2 + o2, isDarkBackground);
        AddQuad(vertices, v3, v0, v0 + o0, v3 + o3, isDarkBackground);
    }

    private static Vector3 GetCorner(Vector3 min, Vector3 max, Int32[] corner)
    {
        return new Vector3(
            min.X * corner[0] + max.X * (1 - corner[0]),
            min.Y * corner[1] + max.Y * (1 - corner[1]),
            min.Z * corner[2] + max.Z * (1 - corner[2])
        );
    }

    private static void AddQuad(PooledList<EffectVertex> vertices, Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3, Boolean isDarkBackground)
    {
        UInt32 data = isDarkBackground.ToUInt();

        vertices.Add(new EffectVertex {Position = v0, Data = data});
        vertices.Add(new EffectVertex {Position = v1, Data = data});
        vertices.Add(new EffectVertex {Position = v2, Data = data});

        vertices.Add(new EffectVertex {Position = v0, Data = data});
        vertices.Add(new EffectVertex {Position = v2, Data = data});
        vertices.Add(new EffectVertex {Position = v3, Data = data});
    }

    /// <summary>
    ///     Call on every logic update.
    /// </summary>
    public void LogicUpdate()
    {
        pipeline.UpdateData();
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed)
            return;

        if (disposing)
            effect.Dispose();

        disposed = true;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~TargetingBoxEffect()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
