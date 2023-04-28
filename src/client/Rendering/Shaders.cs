// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System.Collections.Generic;
using System.IO;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Logging;
using VoxelGame.Support.Definition;
using VoxelGame.Support.Graphics.Objects;
using VoxelGame.Support.Graphics.Utility;
using VoxelGame.Support.Objects;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A utility class for loading, compiling and managing shaders used by the game.
/// </summary>
public sealed class Shaders
{
    private const string SectionFragmentShader = "section";

    private const string TimeUniform = "time";
    private const string NearPlaneUniform = "nearPlane";
    private const string FarPlaneUniform = "farPlane";

    private readonly DirectoryInfo directory;

    private readonly ISet<Shader> farPlaneSet = new HashSet<Shader>();

    private readonly ShaderLoader loader;
    private readonly LoadingContext loadingContext;
    private readonly ISet<Shader> nearPlaneSet = new HashSet<Shader>();

    private readonly ISet<Shader> timedSet = new HashSet<Shader>();

    private bool loaded;
    private RasterPipeline postProcessingPipeline = null!;

    private Shaders(DirectoryInfo directory, LoadingContext loadingContext)
    {
        this.directory = directory;
        this.loadingContext = loadingContext;

        // todo: integrate the new HLSL shaders into the current loading system, then delete the old GLSL shaders
        // todo: when loading the shaders, both UI and Post shader should be integrated into the loading system,
        // but loading failure is fatal and should throw an unhandled exception,
        // for the Post shader the native client needs a method that is called during init and receives the post render pipeline as a parameter

        loader = new ShaderLoader(
            directory,
            loadingContext,
            (timedSet, TimeUniform),
            (nearPlaneSet, NearPlaneUniform),
            (farPlaneSet, FarPlaneUniform));
    }

    /// <summary>
    ///     The shader used for simple blocks.
    /// </summary>
    public Shader SimpleSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for complex blocks.
    /// </summary>
    public Shader ComplexSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for varying height blocks.
    /// </summary>
    public Shader VaryingHeightSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for cross plant blocks.
    /// </summary>
    public Shader CrossPlantSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for crop plant blocks.
    /// </summary>
    public Shader CropPlantSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for opaque fluids.
    /// </summary>
    public Shader OpaqueFluidSection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for the accumulate pass for transparent fluids.
    /// </summary>
    public Shader TransparentFluidSectionAccumulate { get; private set; } = null!;

    /// <summary>
    ///     The shader used for the draw pass for transparent fluids.
    /// </summary>
    public Shader TransparentFluidSectionDraw { get; private set; } = null!;

    /// <summary>
    ///     The shader used for block/fluid texture overlays.
    /// </summary>
    public Shader Overlay { get; private set; } = null!;

    /// <summary>
    ///     The shader used for the selection box.
    /// </summary>
    public Shader Selection { get; private set; } = null!;

    /// <summary>
    ///     The shader used for simply screen elements.
    /// </summary>
    public Shader ScreenElement { get; private set; } = null!;

    /// <summary>
    ///     Load all shaders in the given directory.
    /// </summary>
    /// <param name="directory">The directory containing all shaders.</param>
    /// <param name="client">The client to use.</param>
    /// <param name="loadingContext">The loader to use.</param>
    /// <returns>An object representing all loaded shaders.</returns>
    internal static Shaders Load(DirectoryInfo directory, Support.Client client, LoadingContext loadingContext)
    {
        Shaders shaders = new(directory, loadingContext);

        using (loadingContext.BeginStep(Events.ShaderSetup, "Shader Setup"))
        {
            shaders.LoadAll(client);
        }

        return shaders;
    }

    internal void Delete()
    {
        // todo: think about deleting (some cleanup like removing from draw2d and similar is necessary)
        // todo: maybe some more cleanup would be nice

        return;

        SimpleSection.Delete();
        ComplexSection.Delete();
        VaryingHeightSection.Delete();
        CrossPlantSection.Delete();
        CropPlantSection.Delete();
        OpaqueFluidSection.Delete();
        TransparentFluidSectionAccumulate.Delete();
        TransparentFluidSectionDraw.Delete();

        Overlay.Delete();
        Selection.Delete();
        ScreenElement.Delete();

        loaded = false;
    }

    private void LoadAll(Support.Client client)
    {
        loaded = true;

        RasterPipeline LoadPipeline(string name, ShaderPreset preset)
        {
            FileInfo path = directory.GetFile($"{name}.hlsl");

            RasterPipeline pipeline = client.CreateRasterPipeline(
                PipelineDescription.Create(path, preset),
                error =>
                {
                    loadingContext.ReportFailure(Events.ShaderError, nameof(RasterPipeline), path, error);
                    loaded = false;
                });

            if (loaded) loadingContext.ReportSuccess(Events.ShaderSetup, nameof(RasterPipeline), path);

            return pipeline;
        }

        (RasterPipeline, ShaderBuffer<T>) LoadPipelineWithBuffer<T>(string name, ShaderPreset preset)
            where T : unmanaged
        {
            FileInfo path = directory.GetFile($"{name}.hlsl");

            (RasterPipeline, ShaderBuffer<T>) result = client.CreateRasterPipeline<T>(
                PipelineDescription.Create(path, preset),
                error =>
                {
                    loadingContext.ReportFailure(Events.ShaderError, nameof(RasterPipeline), path, error);
                    loaded = false;
                });

            if (loaded) loadingContext.ReportSuccess(Events.ShaderSetup, nameof(RasterPipeline), path);

            return result;
        }

        postProcessingPipeline = LoadPipeline("Post", ShaderPreset.PostProcessing);

        if (!loaded) return;

        client.SetPostProcessingPipeline(postProcessingPipeline);

        return; // todo: remove this, and maybe the code below

        Shader Check(Shader? shader)
        {
            loaded &= shader != null;

            return shader!;
        }

        SimpleSection = Check(loader.Load(nameof(SimpleSection), "simple_section", SectionFragmentShader));
        ComplexSection = Check(loader.Load(nameof(ComplexSection), "complex_section", SectionFragmentShader));
        VaryingHeightSection = Check(loader.Load(nameof(VaryingHeightSection), "varying_height_section", SectionFragmentShader));
        CrossPlantSection = Check(loader.Load(nameof(CrossPlantSection), "cross_plant_section", SectionFragmentShader));
        CropPlantSection = Check(loader.Load(nameof(CropPlantSection), "crop_plant_section", SectionFragmentShader));
        OpaqueFluidSection = Check(loader.Load(nameof(OpaqueFluidSection), "fluid_section", "opaque_fluid_section"));
        TransparentFluidSectionAccumulate = Check(loader.Load(nameof(TransparentFluidSectionAccumulate), "fluid_section", "transparent_fluid_section_accumulate"));
        TransparentFluidSectionDraw = Check(loader.Load(nameof(TransparentFluidSectionDraw), "fullscreen", "transparent_fluid_section_draw"));

        Overlay = Check(loader.Load(nameof(Overlay), "overlay", "overlay"));
        Selection = Check(loader.Load(nameof(Selection), "selection", "selection"));
        ScreenElement = Check(loader.Load(nameof(ScreenElement), "screen_element", "screen_element"));

        UpdateOrthographicProjection();
    }

    /// <summary>
    ///     Update all orthographic projection matrices.
    /// </summary>
    public void UpdateOrthographicProjection()
    {
        if (!loaded) return;

        return; // todo: remove the return, and maybe the code below (as projection matrix could be known in c++ code)

        Overlay.SetMatrix4(
            "projection",
            Matrix4d.CreateOrthographic(width: 1.0, 1.0 / Screen.AspectRatio, depthNear: 0.0, depthFar: 1.0).ToMatrix4());

        ScreenElement.SetMatrix4(
            "projection",
            Matrix4d.CreateOrthographic(Screen.Size.X, Screen.Size.Y, depthNear: 0.0, depthFar: 1.0).ToMatrix4());
    }

    /// <summary>
    ///     Update the current time.
    /// </summary>
    /// <param name="time">The current time, since the game has started.</param>
    public void SetTime(float time)
    {
        if (!loaded) return;

        foreach (Shader shader in timedSet) shader.SetFloat(TimeUniform, time);
    }

    /// <summary>
    ///     Set the view plane distances.
    /// </summary>
    /// <param name="near">The near plane distance.</param>
    /// <param name="far">The far plane distance.</param>
    public void SetPlanes(double near, double far)
    {
        if (!loaded) return;

        foreach (Shader shader in nearPlaneSet) shader.SetFloat(NearPlaneUniform, (float) near);

        foreach (Shader shader in farPlaneSet) shader.SetFloat(FarPlaneUniform, (float) far);
    }
}
