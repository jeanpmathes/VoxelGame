// <copyright file="Shaders.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

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

        public static Shader SimpleSectionShader { get; private set; } = null!;
        public static Shader ComplexSectionShader { get; private set; } = null!;
        public static Shader VaryingHeightShader { get; private set; } = null!;
        public static Shader OpaqueLiquidSectionShader { get; private set; } = null!;
        public static Shader TransparentLiquidSectionShader { get; private set; } = null!;
        public static Shader OverlayShader { get; private set; } = null!;
        public static Shader SelectionShader { get; private set; } = null!;
        public static Shader ScreenElementShader { get; private set; } = null!;

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
                SimpleSectionShader = loader.Load("simple_section.vert", "section.frag");
                ComplexSectionShader = loader.Load("complex_section.vert", "section.frag");
                VaryingHeightShader = loader.Load("varying_height_section.vert", "section.frag");
                OpaqueLiquidSectionShader = loader.Load("liquid_section.vert", "opaque_liquid_section.frag");
                TransparentLiquidSectionShader = loader.Load("liquid_section.vert", "transparent_liquid_section.frag");
                OverlayShader = loader.Load("overlay.vert", "overlay.frag");
                SelectionShader = loader.Load("selection.vert", "selection.frag");
                ScreenElementShader = loader.Load("screen_element.vert", "screen_element.frag");

                OverlayShader.SetMatrix4("projection", Matrix4.CreateOrthographic(1f, 1f / Screen.AspectRatio, 0f, 1f));
                ScreenElementShader.SetMatrix4("projection", Matrix4.CreateOrthographic(Screen.Size.X, Screen.Size.Y, 0f, 1f));

                Logger.LogInformation("Shader setup complete.");
            }
        }
    }
}