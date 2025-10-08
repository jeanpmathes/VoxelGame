// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Logic.Sections;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Graphics.Core;
using VoxelGame.Graphics.Data;
using VoxelGame.Graphics.Graphics.Raytracing;
using VoxelGame.Graphics.Objects;
using VoxelGame.Toolkit.Utilities;
using Mesh = VoxelGame.Graphics.Objects.Mesh;

namespace VoxelGame.Client.Visuals;

/// <summary>
///     Renders a <see cref="Section" />.
/// </summary>
public sealed class SectionRenderer : IDisposable
{
    /// <summary>
    ///     The basic raytracing material for opaque section parts.
    /// </summary>
    private static Material basicOpaqueMaterial = null!;

    /// <summary>
    ///     The basic raytracing material for transparent section parts.
    /// </summary>
    private static Material basicTransparentMaterial = null!;

    /// <summary>
    ///     The raytracing material used for foliage.
    /// </summary>
    private static Material foliageMaterial = null!;

    /// <summary>
    ///     The raytracing material used for opaque fluids.
    /// </summary>
    private static Material fluidMaterial = null!;

    private readonly Vector3d position;

    private readonly Space space;
    private (Mesh? opaque, Mesh? transparent) basic;
    private Boolean enabled;
    private Mesh? fluid;
    private Mesh? foliage;

    /// <summary>
    ///     Creates a new <see cref="SectionRenderer" />.
    /// </summary>
    public SectionRenderer(Space space, Vector3d position)
    {
        this.space = space;
        this.position = position;
    }

    /// <summary>
    ///     Get or set whether the section renderer is enabled.
    /// </summary>
    public Boolean IsEnabled
    {
        get => enabled;
        set
        {
            enabled = value;
            UpdateEnabledState();
        }
    }

    /// <summary>
    ///     Initialize the required resources for the <see cref="SectionRenderer" />.
    /// </summary>
    /// <param name="directory">The directory in which shader files are located.</param>
    /// <param name="visuals">The visual configuration of the game.</param>
    /// <param name="builder">The pipeline builder that is used to build the raytracing pipeline.</param>
    public static void InitializeRequiredResources(DirectoryInfo directory, VisualConfiguration visuals, PipelineBuilder builder)
    {
        PipelineBuilder.HitGroup basicOpaqueSectionHitGroup = new("BasicOpaqueSectionClosestHit");
        PipelineBuilder.HitGroup basicOpaqueShadowHitGroup = new("BasicOpaqueShadowClosestHit");

        PipelineBuilder.HitGroup basicTransparentSectionHitGroup = new("BasicTransparentSectionClosestHit", "BasicTransparentSectionAnyHit");
        PipelineBuilder.HitGroup basicTransparentShadowHitGroup = new("BasicTransparentShadowClosestHit", "BasicTransparentShadowAnyHit");

        PipelineBuilder.HitGroup foliageSectionHitGroup = new("FoliageSectionClosestHit", "FoliageSectionAnyHit");
        PipelineBuilder.HitGroup foliageShadowHitGroup = new("FoliageShadowClosestHit", "FoliageShadowAnyHit");

        PipelineBuilder.HitGroup fluidSectionHitGroup = new("FluidSectionClosestHit");
        PipelineBuilder.HitGroup fluidShadowHitGroup = new("FluidShadowClosestHit");

        builder.AddShaderFile(directory.GetFile("BasicOpaque.hlsl"), [basicOpaqueSectionHitGroup, basicOpaqueShadowHitGroup]);
        builder.AddShaderFile(directory.GetFile("BasicTransparent.hlsl"), [basicTransparentSectionHitGroup, basicTransparentShadowHitGroup]);
        builder.AddShaderFile(directory.GetFile("Foliage.hlsl"), [foliageSectionHitGroup, foliageShadowHitGroup]);
        builder.AddShaderFile(directory.GetFile("Fluid.hlsl"), [fluidSectionHitGroup, fluidShadowHitGroup]);

        basicOpaqueMaterial = builder.AddMaterial(
            nameof(basicOpaqueMaterial),
            PipelineBuilder.Groups.Default,
            isOpaque: true,
            basicOpaqueSectionHitGroup,
            basicOpaqueShadowHitGroup);

        basicTransparentMaterial = builder.AddMaterial(
            nameof(basicTransparentMaterial),
            PipelineBuilder.Groups.Default,
            isOpaque: false,
            basicTransparentSectionHitGroup,
            basicTransparentShadowHitGroup);

        foliageMaterial = builder.AddMaterial(
            nameof(foliageMaterial),
            PipelineBuilder.Groups.Default,
            isOpaque: false,
            foliageSectionHitGroup,
            foliageShadowHitGroup,
            visuals.FoliageQuality > Quality.Low ? builder.AddAnimation(directory.GetFile("FoliageAnimation.hlsl")) : null);

        fluidMaterial = builder.AddMaterial(
            nameof(fluidMaterial),
            PipelineBuilder.Groups.NoShadow,
            isOpaque: true, // Despite having transparency, no any-hit shader is used, so it is considered opaque.
            fluidSectionHitGroup,
            fluidShadowHitGroup);
    }

    private void UpdateEnabledState()
    {
        if (basic.opaque != null)
            basic.opaque.IsEnabled = enabled;

        if (basic.transparent != null)
            basic.transparent.IsEnabled = enabled;

        if (foliage != null)
            foliage.IsEnabled = enabled;

        if (fluid != null)
            fluid.IsEnabled = enabled;
    }

    private Mesh CreateMesh(Material material)
    {
        Mesh mesh = space.CreateMesh(material, position);

        mesh.IsEnabled = enabled;

        return mesh;
    }

    /// <summary>
    ///     Set the section mesh data to display. Must not be discarded.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    public void SetData(SectionMeshData meshData)
    {
        Throw.IfDisposed(disposed);
        Core.App.Application.ThrowIfNotOnMainThread(this);

        if (meshData.BasicMeshing.opaque.Count > 0 || basic.opaque != null)
        {
            basic.opaque ??= CreateMesh(basicOpaqueMaterial);
            basic.opaque.SetVertices((meshData.BasicMeshing.opaque as SpatialMeshing)!.Span);
        }

        if (meshData.BasicMeshing.transparent.Count > 0 || basic.transparent != null)
        {
            basic.transparent ??= CreateMesh(basicTransparentMaterial);
            basic.transparent.SetVertices((meshData.BasicMeshing.transparent as SpatialMeshing)!.Span);
        }

        if (meshData.FoliageMeshing.Count > 0 || foliage != null)
        {
            foliage ??= CreateMesh(foliageMaterial);
            foliage.SetVertices((meshData.FoliageMeshing as SpatialMeshing)!.Span);
        }

        if (meshData.FluidMeshing.Count > 0 || fluid != null)
        {
            fluid ??= CreateMesh(fluidMaterial);
            fluid.SetVertices((meshData.FluidMeshing as SpatialMeshing)!.Span);
        }
    }

    #region DISPOSABLE

    private Boolean disposed;

    private void Dispose(Boolean disposing)
    {
        if (disposed) return;

        if (disposing)
        {
            basic.opaque?.Dispose();
            basic.transparent?.Dispose();

            foliage?.Dispose();
            fluid?.Dispose();
        }
        else
        {
            Throw.ForMissedDispose(nameof(SectionRenderer));
        }

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
    ~SectionRenderer()
    {
        Dispose(disposing: false);
    }

    #endregion DISPOSABLE
}
