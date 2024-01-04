// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Data;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer that renders instances of the <see cref="BoxCollider" /> struct.
///     For this multiple boxes are drawn.
/// </summary>
public sealed class BoxRenderer : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<BoxRenderer>();

    private readonly Effect effect;

    private BoxCollider? currentBox;

    /// <summary>
    ///     Create a new <see cref="BoxRenderer" />.
    /// </summary>
    public BoxRenderer(Space space)
    {
        effect = space.CreateEffect(Pipelines.SelectionEffect);
    }

    private static Pipelines Pipelines => Application.Client.Instance.Resources.Pipelines;

    /// <summary>
    /// Set whether the renderer is enabled.
    /// </summary>
    public bool IsEnabled
    {
        get => effect.IsEnabled;
        set => effect.IsEnabled = value;
    }

    /// <summary>
    ///     Set the box collider to render.
    /// </summary>
    /// <param name="boxCollider">The box collider to render.</param>
    public void SetBox(BoxCollider boxCollider)
    {
        if (disposed) return;

        if (currentBox == boxCollider) return;

        currentBox = boxCollider;
        effect.Position = boxCollider.Position;

        PooledList<EffectVertex> vertices = new();
        BuildMeshData(boxCollider.Volume, vertices);

        {
            // todo: remove the block
            var c = boxCollider.Volume.Center.ToVector3();
            vertices.Clear();
            vertices.Add(new EffectVertex {Position = c + (0, 1, 0), Data = 0});
            vertices.Add(new EffectVertex {Position = c + (0, 0, 0), Data = 0});
            vertices.Add(new EffectVertex {Position = c + (1, 1, 0), Data = 0});
        }

        effect.SetNewVertices(vertices.AsSpan());
        vertices.ReturnToPool();
    }

    private static void BuildMeshData(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        BuildMeshDataForTopLevelBox(boundingVolume, vertices);

        if (boundingVolume.ChildCount == 0) return;

        for (var i = 0; i < boundingVolume.ChildCount; i++)
        {
            BuildMeshData(boundingVolume[i], vertices);
        }
    }

    private static void BuildMeshDataForTopLevelBox(BoundingVolume boundingVolume, PooledList<EffectVertex> vertices)
    {
        (float minX, float minY, float minZ) = boundingVolume.Min.ToVector3();
        (float maxX, float maxY, float maxZ) = boundingVolume.Max.ToVector3();

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

    private static void AddLine(PooledList<EffectVertex> vertices, Vector3 a, Vector3 b)
    {
        vertices.Add(new EffectVertex {Position = a, Data = 0});
        vertices.Add(new EffectVertex {Position = b, Data = 0});
    }

    /// <summary>
    ///     Data used by the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Data : IEquatable<Data>
    {
        /// <summary>
        ///     The color of the rendered boxes.
        /// </summary>
        public Vector3 Color;

        /// <summary>
        ///     Check equality.
        /// </summary>
        public bool Equals(Data other)
        {
            return Color.Equals(other.Color);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Data other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Color.GetHashCode();
        }

        /// <summary>
        ///     The equality operator.
        /// </summary>
        public static bool operator ==(Data left, Data right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     The inequality operator.
        /// </summary>
        public static bool operator !=(Data left, Data right)
        {
            return !left.Equals(right);
        }
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing) effect.Return();
        else
            logger.LogWarning(
                Events.UndeletedBuffers,
                "Renderer disposed by GC without freeing storage");

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~BoxRenderer()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of this renderer.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}
