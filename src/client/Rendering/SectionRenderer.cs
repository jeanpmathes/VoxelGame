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

    private readonly Space space;
    private readonly Vector3d position;

    private (MeshObject? opaque, MeshObject? transparent) basic;
    private MeshObject? foliage;
    private MeshObject? fluid;

    /// <summary>
    ///     Creates a new <see cref="SectionRenderer" />.
    /// </summary>
    public SectionRenderer(Space space, Vector3d position)
    {
        this.space = space;
        this.position = position;
    }

    private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

    /// <summary>
    ///     Set the section mesh data to render. Must not be discarded.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    public void SetData(SectionMeshData meshData)
    {
        if (disposed) return;

        if (meshData.BasicMesh.opaque.Count > 0 || basic.opaque != null)
        {
            basic.opaque ??= space.CreateMeshObject(Shaders.BasicOpaqueSectionMaterial, position);
            basic.opaque.SetVertices(meshData.BasicMesh.opaque.AsSpan());
        }

        if (meshData.BasicMesh.transparent.Count > 0 || basic.transparent != null)
        {
            basic.transparent ??= space.CreateMeshObject(Shaders.BasicTransparentSectionMaterial, position);
            basic.transparent.SetVertices(meshData.BasicMesh.transparent.AsSpan());
        }

        if (meshData.FoliageMesh.Count > 0 || foliage != null)
        {
            foliage ??= space.CreateMeshObject(Shaders.FoliageSectionMaterial, position);
            foliage.SetVertices(meshData.FoliageMesh.AsSpan());
        }

        if (meshData.FluidMesh.Count > 0 || fluid != null)
        {
            fluid ??= space.CreateMeshObject(Shaders.FluidSectionMaterial, position);
            fluid.SetVertices(meshData.FluidMesh.AsSpan());
        }

        meshData.ReturnPooled();
    }

    /// <summary>
    ///     Set whether the renderer is enabled.
    /// </summary>
    public void SetEnabledState(bool enabled)
    {
        if (disposed) return;

        if (basic.opaque != null)
            basic.opaque.IsEnabled = enabled;

        if (basic.transparent != null)
            basic.transparent.IsEnabled = enabled;

        if (foliage != null)
            foliage.IsEnabled = enabled;

        if (fluid != null)
            fluid.IsEnabled = enabled;
    }

    #region IDisposable Support

    private bool disposed;

    private void Dispose(bool disposing)
    {
        if (disposed)
            return;

        if (disposing)
        {
            basic.opaque?.Return();
            basic.transparent?.Return();

            foliage?.Return();
            fluid?.Return();
        }
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
