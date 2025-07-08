// <copyright file="TargetingBoxEffect.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Physics;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Objects;

namespace VoxelGame.Client.Visuals;

/// <summary>
/// A rendering effect for visualizing targeting boxes in the world.
/// </summary>
public sealed class TargetingBoxEffect : IDisposable
{
    private readonly Effect effect;
    private readonly TargetingBoxPipeline pipeline;
    
    private BoxCollider? currentBox;

    internal TargetingBoxEffect(Effect effect, TargetingBoxPipeline pipeline)
    {
        this.effect = effect;
        this.pipeline = pipeline;
    }

    /// <summary>
    /// Get or set whether the effect is enabled.
    /// </summary>
    public Boolean IsEnabled
    {
        get => effect.IsEnabled;
        set => effect.IsEnabled = value;
    }
    
        /// <summary>
    ///     Set the box collider to display.
    /// </summary>
    /// <param name="boxCollider">The box collider to display.</param>
    public void SetBox(BoxCollider boxCollider)
    {
        if (currentBox == boxCollider) return;

        currentBox = boxCollider;
        effect.Position = boxCollider.Position;

        using PooledList<EffectVertex> vertices = new();
        BuildMeshData(boxCollider.Volume, vertices);

        effect.SetNewVertices(vertices.AsSpan());
    }

    private static void BuildMeshData(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        BuildMeshDataForTopLevelBox(boundingVolume, vertices);

        if (boundingVolume.ChildCount == 0) return;

        for (var i = 0; i < boundingVolume.ChildCount; i++) BuildMeshData(boundingVolume[i], vertices);
    }

    private static void BuildMeshDataForTopLevelBox(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        (Single minX, Single minY, Single minZ) = (Vector3) boundingVolume.Min;
        (Single maxX, Single maxY, Single maxZ) = (Vector3) boundingVolume.Max;

        // The four bottom lines:
        AddLine(vertices, (minX, minY, minZ), (maxX, minY, minZ));
        AddLine(vertices, (minX, minY, maxZ), (maxX, minY, maxZ));
        AddLine(vertices, (minX, minY, minZ), (minX, minY, maxZ));
        AddLine(vertices, (maxX, minY, minZ), (maxX, minY, maxZ));

        // The four top lines:
        AddLine(vertices, (minX, maxY, minZ), (maxX, maxY, minZ));
        AddLine(vertices, (minX, maxY, maxZ), (maxX, maxY, maxZ));
        AddLine(vertices, (minX, maxY, minZ), (minX, maxY, maxZ));
        AddLine(vertices, (maxX, maxY, minZ), (maxX, maxY, maxZ));

        // The four vertical lines:
        AddLine(vertices, (minX, minY, minZ), (minX, maxY, minZ));
        AddLine(vertices, (maxX, minY, minZ), (maxX, maxY, minZ));
        AddLine(vertices, (minX, minY, maxZ), (minX, maxY, maxZ));
        AddLine(vertices, (maxX, minY, maxZ), (maxX, maxY, maxZ));
    }
    
#pragma warning disable S3242 // Concrete type used for performance.
    private static void AddLine(PooledList<EffectVertex> vertices, Vector3 a, Vector3 b)
#pragma warning restore S3242
    {
        vertices.Add(new EffectVertex {Position = a, Data = 0});
        vertices.Add(new EffectVertex {Position = b, Data = 0});
    }
    
    /// <summary>
    /// Call on every logic update.
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
    /// Finalizer.
    /// </summary>
    ~TargetingBoxEffect()
    {
        Dispose(disposing: false);
    }
    
    #endregion DISPOSABLE
}
