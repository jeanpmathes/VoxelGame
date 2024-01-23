﻿// <copyright file="SectionRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.IO;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;
using VoxelGame.Logging;
using VoxelGame.Support.Core;
using VoxelGame.Support.Data;
using VoxelGame.Support.Graphics.Raytracing;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A renderer for <see cref="VoxelGame.Core.Logic.Section" />.
/// </summary>
public sealed class SectionRenderer : Renderer
{
    private static readonly ILogger logger = LoggingHelper.CreateLogger<SectionRenderer>();

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

    private readonly Space space;
    private readonly Vector3d position;

    private bool enabled;

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

    /// <inheritdoc />
    public override bool IsEnabled
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

        builder.AddShaderFile(directory.GetFile("BasicOpaque.hlsl"), new[] {basicOpaqueSectionHitGroup, basicOpaqueShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("BasicTransparent.hlsl"), new[] {basicTransparentSectionHitGroup, basicTransparentShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("Foliage.hlsl"), new[] {foliageSectionHitGroup, foliageShadowHitGroup});
        builder.AddShaderFile(directory.GetFile("Fluid.hlsl"), new[] {fluidSectionHitGroup, fluidShadowHitGroup});

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

    /// <inheritdoc />
    protected override void OnSetUp()
    {
        // Intentionally left empty.
    }

    /// <inheritdoc />
    protected override void OnTearDown()
    {
        // Intentionally left empty.
    }

    /// <summary>
    ///     Set the section mesh data to render. Must not be discarded.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    public void SetData(SectionMeshData meshData)
    {
        if (meshData.BasicMeshing.opaque.Count > 0 || basic.opaque != null)
        {
            basic.opaque ??= space.CreateMesh(basicOpaqueMaterial, position);
            basic.opaque.SetVertices((meshData.BasicMeshing.opaque as SpatialMeshing)!.Span);
        }

        if (meshData.BasicMeshing.transparent.Count > 0 || basic.transparent != null)
        {
            basic.transparent ??= space.CreateMesh(basicTransparentMaterial, position);
            basic.transparent.SetVertices((meshData.BasicMeshing.transparent as SpatialMeshing)!.Span);
        }

        if (meshData.FoliageMeshing.Count > 0 || foliage != null)
        {
            foliage ??= space.CreateMesh(foliageMaterial, position);
            foliage.SetVertices((meshData.FoliageMeshing as SpatialMeshing)!.Span);
        }

        if (meshData.FluidMeshing.Count > 0 || fluid != null)
        {
            fluid ??= space.CreateMesh(fluidMaterial, position);
            fluid.SetVertices((meshData.FluidMeshing as SpatialMeshing)!.Span);
        }

        meshData.Release();
    }

    #region IDisposable Support

    /// <inheritdoc />
    protected override void OnDispose(bool disposing)
    {
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
    }

    #endregion IDisposable Support
}
