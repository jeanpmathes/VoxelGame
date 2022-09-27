// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using VoxelGame.Core.Utilities;
using VoxelGame.Graphics.Objects;
using VoxelGame.Graphics.Utility;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering;

/// <summary>
///     A utility class for loading, compiling and managing shaders used by the game.
/// </summary>
public sealed class Shaders
{
    private const string SectionFragmentShader = "section.frag";

    private const string TimeUniform = "time";
    private const string NearPlaneUniform = "nearPlane";
    private const string FarPlaneUniform = "farPlane";

    private static readonly ILogger logger = LoggingHelper.CreateLogger<Shaders>();
    private readonly ISet<Shader> farPlaneSet = new HashSet<Shader>();

    private readonly ShaderLoader loader;
    private readonly ISet<Shader> nearPlaneSet = new HashSet<Shader>();

    private readonly ISet<Shader> timedSet = new HashSet<Shader>();

    private Shaders(string directory)
    {
        loader = new ShaderLoader(
            directory,
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
    /// <returns>An object representing all loaded shaders.</returns>
    internal static Shaders Load(string directory)
    {
        Shaders shaders = new(directory);
        shaders.LoadAll();

        return shaders;
    }

    internal void Delete()
    {
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
    }

    private void LoadAll()
    {
        using (logger.BeginScope("Shader setup"))
        {
            loader.LoadIncludable("noise", "noise.glsl");
            loader.LoadIncludable("decode", "decode.glsl");
            loader.LoadIncludable("color", "color.glsl");

            SimpleSection = loader.Load("simple_section.vert", SectionFragmentShader);
            ComplexSection = loader.Load("complex_section.vert", SectionFragmentShader);
            VaryingHeightSection = loader.Load("varying_height_section.vert", SectionFragmentShader);
            CrossPlantSection = loader.Load("cross_plant_section.vert", SectionFragmentShader);
            CropPlantSection = loader.Load("crop_plant_section.vert", SectionFragmentShader);
            OpaqueFluidSection = loader.Load("fluid_section.vert", "opaque_fluid_section.frag");
            TransparentFluidSectionAccumulate = loader.Load("fluid_section.vert", "transparent_fluid_section_accumulate.frag");
            TransparentFluidSectionDraw = loader.Load("fullscreen.vert", "transparent_fluid_section_draw.frag");

            Overlay = loader.Load("overlay.vert", "overlay.frag");
            Selection = loader.Load("selection.vert", "selection.frag");
            ScreenElement = loader.Load("screen_element.vert", "screen_element.frag");

            UpdateOrthographicProjection();

            logger.LogInformation(Events.ShaderSetup, "Completed shader setup");
        }
    }

    /// <summary>
    ///     Update all orthographic projection matrices.
    /// </summary>
    public void UpdateOrthographicProjection()
    {
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
        foreach (Shader shader in timedSet) shader.SetFloat(TimeUniform, time);
    }

    /// <summary>
    ///     Set the view plane distances.
    /// </summary>
    /// <param name="near">The near plane distance.</param>
    /// <param name="far">The far plane distance.</param>
    public void SetPlanes(double near, double far)
    {
        foreach (Shader shader in nearPlaneSet) shader.SetFloat(NearPlaneUniform, (float) near);

        foreach (Shader shader in farPlaneSet) shader.SetFloat(FarPlaneUniform, (float) far);
    }
}
