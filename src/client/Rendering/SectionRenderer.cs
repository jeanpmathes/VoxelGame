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

    private readonly (MeshObject opaque, MeshObject transparent) basic;
    private readonly MeshObject foliage;
    private readonly MeshObject fluid;

    /// <summary>
    ///     Creates a new <see cref="SectionRenderer" />.
    /// </summary>
    public SectionRenderer(Space space, Vector3d position)
    {
        basic = (
            space.CreateMeshObject(Shaders.BasicOpaqueSectionMaterial, position),
            space.CreateMeshObject(Shaders.BasicTransparentSectionMaterial, position)
        );

        foliage = space.CreateMeshObject(Shaders.FoliageSectionMaterial, position);
        fluid = space.CreateMeshObject(Shaders.FluidSectionMaterial, position);
    }

    private static Shaders Shaders => Application.Client.Instance.Resources.Shaders;

    /// <summary>
    ///     Set the section mesh data to render. Must not be discarded.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    public void SetData(SectionMeshData meshData)
    {
        if (disposed) return;

        basic.opaque.SetVertices(meshData.BasicMesh.opaque.AsSpan());
        basic.transparent.SetVertices(meshData.BasicMesh.transparent.AsSpan());

        foliage.SetVertices(meshData.FoliageMesh.AsSpan());
        fluid.SetVertices(meshData.FluidMesh.AsSpan());

        meshData.ReturnPooled();
    }

    /// <summary>
    ///     Set whether the renderer is enabled.
    /// </summary>
    public void SetEnabledState(bool enabled)
    {
        if (disposed) return;

        basic.opaque.IsEnabled = enabled;
        basic.transparent.IsEnabled = enabled;

        foliage.IsEnabled = enabled;
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
            basic.opaque.Free();
            basic.transparent.Free();

            foliage.Free();
            fluid.Free();
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
