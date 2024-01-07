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
using VoxelGame.Support.Core;
using VoxelGame.Support.Data;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer for <see cref="VoxelGame.Core.Logic.Section" />.
/// </summary>
public sealed class SectionRenderer : IDisposable // todo: inherit renderer base class, call the methods from users
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionRenderer>();

    private readonly Space space;
    private readonly Vector3d position;

    private (Mesh? opaque, Mesh? transparent) basic;
    private Mesh? foliage;
    private Mesh? fluid;

    /// <summary>
    ///     Creates a new <see cref="SectionRenderer" />.
    /// </summary>
    public SectionRenderer(Space space, Vector3d position)
    {
        this.space = space;
        this.position = position;
    }

    private static Pipelines Pipelines => Application.Client.Instance.Resources.Pipelines;

    /// <summary>
    ///     Set the section mesh data to render. Must not be discarded.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    public void SetData(SectionMeshData meshData)
    {
        if (disposed) return;

        if (meshData.BasicMeshing.opaque.Count > 0 || basic.opaque != null)
        {
            basic.opaque ??= space.CreateMesh(Pipelines.BasicOpaqueSectionMaterial, position);
            basic.opaque.SetVertices((meshData.BasicMeshing.opaque as SpatialMeshing)!.Span);
        }

        if (meshData.BasicMeshing.transparent.Count > 0 || basic.transparent != null)
        {
            basic.transparent ??= space.CreateMesh(Pipelines.BasicTransparentSectionMaterial, position);
            basic.transparent.SetVertices((meshData.BasicMeshing.transparent as SpatialMeshing)!.Span);
        }

        if (meshData.FoliageMeshing.Count > 0 || foliage != null)
        {
            foliage ??= space.CreateMesh(Pipelines.FoliageSectionMaterial, position);
            foliage.SetVertices((meshData.FoliageMeshing as SpatialMeshing)!.Span);
        }

        if (meshData.FluidMeshing.Count > 0 || fluid != null)
        {
            fluid ??= space.CreateMesh(Pipelines.FluidSectionMaterial, position);
            fluid.SetVertices((meshData.FluidMeshing as SpatialMeshing)!.Span);
        }

        meshData.Release();
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
