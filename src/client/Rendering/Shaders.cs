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
        private static readonly ILogger Logger = LoggingHelper.CreateLogger<Shaders>();

        private static Shaders? _instance;

        public static Shader SimpleSection { get; private set; } = null!;
        public static Shader ComplexSection { get; private set; } = null!;
        public static Shader VaryingHeight { get; private set; } = null!;
        public static Shader CrossPlantSection { get; private set; } = null!;
        public static Shader CropPlantSection { get; private set; } = null!;
        public static Shader OpaqueLiquidSection { get; private set; } = null!;
        public static Shader TransparentLiquidSection { get; private set; } = null!;
        public static Shader Overlay { get; private set; } = null!;
        public static Shader Selection { get; private set; } = null!;
        public static Shader ScreenElement { get; private set; } = null!;

        internal static void Load(string directory)
        {
            _instance ??= new Shaders(directory);
            _instance.LoadAll();
        }

        private readonly ShaderLoader loader;

        private Shaders(string directory)
        {
            loader = new ShaderLoader(directory);
        }

        private void LoadAll()
        {
            using (Logger.BeginScope("Shader setup"))
            {
                SimpleSection = loader.Load("simple_section.vert", "section.frag");
                ComplexSection = loader.Load("complex_section.vert", "section.frag");
                VaryingHeight = loader.Load("varying_height_section.vert", "section.frag");
                CrossPlantSection = loader.Load("cross_plant_section.vert", "section.frag");
                CropPlantSection = loader.Load("crop_plant_section.vert", "section.frag");
                OpaqueLiquidSection = loader.Load("liquid_section.vert", "opaque_liquid_section.frag");
                TransparentLiquidSection = loader.Load("liquid_section.vert", "transparent_liquid_section.frag");

                Overlay = loader.Load("overlay.vert", "overlay.frag");
                Selection = loader.Load("selection.vert", "selection.frag");
                ScreenElement = loader.Load("screen_element.vert", "screen_element.frag");

                UpdateOrthographicProjection();

                Logger.LogInformation("Shader setup complete.");
            }
        }

        public static void UpdateOrthographicProjection()
        {
            Overlay.SetMatrix4("projection", Matrix4.CreateOrthographic(1f, 1f / Screen.AspectRatio, 0f, 1f));
            ScreenElement.SetMatrix4("projection", Matrix4.CreateOrthographic(Screen.Size.X, Screen.Size.Y, 0f, 1f));
        }
    }
}