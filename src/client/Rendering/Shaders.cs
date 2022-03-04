// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OpenToolkit.Mathematics;
using VoxelGame.Graphics.Objects;
using VoxelGame.Graphics.Utility;
using VoxelGame.Logging;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    ///     A utility class for loading, compiling and managing shaders used by the game.
    /// </summary>
    public sealed class Shaders
    {
        private const string SectionFragmentShader = "section.frag";

        private const string TimeUniform = "time";
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Shaders>();

        private readonly ShaderLoader loader;

        private readonly ISet<Shader> timedSet = new HashSet<Shader>();

        private Shaders(string directory)
        {
            loader = new ShaderLoader(directory, (timedSet, TimeUniform));
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
        ///     The shader used for opaque liquids.
        /// </summary>
        public Shader OpaqueLiquidSection { get; private set; } = null!;

        /// <summary>
        ///     The shader used for transparent liquids.
        /// </summary>
        public Shader TransparentLiquidSection { get; private set; } = null!;

        /// <summary>
        ///     The shader used for block/liquid texture overlays.
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
            OpaqueLiquidSection.Delete();
            TransparentLiquidSection.Delete();

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

                SimpleSection = loader.Load("simple_section.vert", SectionFragmentShader);
                ComplexSection = loader.Load("complex_section.vert", SectionFragmentShader);
                VaryingHeightSection = loader.Load("varying_height_section.vert", SectionFragmentShader);
                CrossPlantSection = loader.Load("cross_plant_section.vert", SectionFragmentShader);
                CropPlantSection = loader.Load("crop_plant_section.vert", SectionFragmentShader);
                OpaqueLiquidSection = loader.Load("liquid_section.vert", "opaque_liquid_section.frag");
                TransparentLiquidSection = loader.Load("liquid_section.vert", "transparent_liquid_section.frag");

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
                Matrix4.CreateOrthographic(width: 1f, 1f / Screen.AspectRatio, depthNear: 0f, depthFar: 1f));

            ScreenElement.SetMatrix4(
                "projection",
                Matrix4.CreateOrthographic(Screen.Size.X, Screen.Size.Y, depthNear: 0f, depthFar: 1f));
        }

        /// <summary>
        ///     Update the current time.
        /// </summary>
        /// <param name="time">The current time, since the game has started.</param>
        public void SetTime(float time)
        {
            foreach (Shader shader in timedSet) shader.SetFloat(TimeUniform, time);
        }
    }
}
