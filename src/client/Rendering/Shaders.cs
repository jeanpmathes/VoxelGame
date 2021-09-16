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
    internal sealed class Shaders
    {
        private const string TimeUniform = "time";
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Shaders>();

        private static Shaders? instance;

        private readonly ShaderLoader loader;

        private readonly ISet<Shader> timedSet = new HashSet<Shader>();

        private Shaders(string directory)
        {
            loader = new ShaderLoader(directory, (timedSet, TimeUniform));
        }

        public static Shader SimpleSection { get; private set; } = null!;
        public static Shader ComplexSection { get; private set; } = null!;
        public static Shader VaryingHeightSection { get; private set; } = null!;
        public static Shader CrossPlantSection { get; private set; } = null!;
        public static Shader CropPlantSection { get; private set; } = null!;
        public static Shader OpaqueLiquidSection { get; private set; } = null!;
        public static Shader TransparentLiquidSection { get; private set; } = null!;
        public static Shader Overlay { get; private set; } = null!;
        public static Shader Selection { get; private set; } = null!;
        public static Shader ScreenElement { get; private set; } = null!;

        internal static void Load(string directory)
        {
            instance ??= new Shaders(directory);
            instance.LoadAll();
        }

        private void LoadAll()
        {
            using (logger.BeginScope("Shader setup"))
            {
                loader.LoadIncludable("noise", "noise.glsl");

                SimpleSection = loader.Load("simple_section.vert", "section.frag");
                ComplexSection = loader.Load("complex_section.vert", "section.frag");
                VaryingHeightSection = loader.Load("varying_height_section.vert", "section.frag");
                CrossPlantSection = loader.Load("cross_plant_section.vert", "section.frag");
                CropPlantSection = loader.Load("crop_plant_section.vert", "section.frag");
                OpaqueLiquidSection = loader.Load("liquid_section.vert", "opaque_liquid_section.frag");
                TransparentLiquidSection = loader.Load("liquid_section.vert", "transparent_liquid_section.frag");

                Overlay = loader.Load("overlay.vert", "overlay.frag");
                Selection = loader.Load("selection.vert", "selection.frag");
                ScreenElement = loader.Load("screen_element.vert", "screen_element.frag");

                UpdateOrthographicProjection();

                logger.LogInformation("Completed shader setup");
            }
        }

        public static void UpdateOrthographicProjection()
        {
            Overlay.SetMatrix4(
                "projection",
                Matrix4.CreateOrthographic(width: 1f, 1f / Screen.AspectRatio, depthNear: 0f, depthFar: 1f));

            ScreenElement.SetMatrix4(
                "projection",
                Matrix4.CreateOrthographic(Screen.Size.X, Screen.Size.Y, depthNear: 0f, depthFar: 1f));
        }

        public static void SetTime(float time)
        {
            if (instance == null) return;

            foreach (Shader shader in instance.timedSet) shader.SetFloat(TimeUniform, time);
        }
    }
}