// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer for <see cref="VoxelGame.Core.Logic.Section" />.
/// </summary>
public sealed class SectionRenderer : IDisposable
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionRenderer>();
    private readonly MeshObject complex;
    private readonly MeshObject cropPlant;

    private readonly MeshObject crossPlant;

    private readonly MeshObject opaqueFluid;

    private readonly MeshObject simple;
    private readonly MeshObject transparentFluid;
    private readonly MeshObject varyingHeight;

    /// <summary>
    ///     Creates a new <see cref="SectionRenderer" />.
    /// </summary>
    public SectionRenderer(Space space, Vector3d position)
    {
        simple = space.CreateMeshObject(Shaders.SimpleSectionMaterial, position);
        complex = null!;
        varyingHeight = null!;

        opaqueFluid = null!;
        transparentFluid = null!;

        crossPlant = null!;
        cropPlant = null!;
    }

    private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

    /// <summary>
    ///     Set the section mesh data to render. Must not be discarded.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    public void SetData(SectionMeshData meshData)
    {
        if (disposed) return;

        simple.SetMesh(meshData.SimpleMesh.AsSpan());

        meshData.ReturnPooled();
    }

    /// <summary>
    ///     Set whether the renderer is enabled.
    /// </summary>
    public void SetEnabledState(bool enabled)
    {
        if (disposed) return;

        simple.IsEnabled = enabled;
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing) simple.Free();
        // todo: free other mesh objects
        else
            logger.LogWarning(
                Events.LeakedNativeObject,
                "Renderer disposed by GC without freeing storage");

        disposed = true;
    }

    /// <summary>
    ///     Finalizer.
    /// </summary>
    ~SectionRenderer()
    {
        Dispose(disposing: false);
    }

    /// <summary>
    ///     Dispose of the renderer.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion IDisposable Support
}
