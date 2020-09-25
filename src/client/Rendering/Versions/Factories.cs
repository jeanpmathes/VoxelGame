// <copyright file="Factories.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Client.Rendering.Versions
{
    internal abstract class ArrayTextureFactory
    {
        internal abstract ArrayTexture CreateArrayTexture(string path, int resolution, bool useCustomMipmapGeneration, params TextureUnit[] textureUnits);
    }

    internal abstract class BoxRendererFactory
    {
        internal abstract BoxRenderer CreateBoxRenderer();
    }

    internal abstract class ScreenFactory
    {
        internal abstract Screen CreateScreen(Client client);
    }

    internal abstract class ScreenElementRendererFactory
    {
        internal abstract ScreenElementRenderer CreateScreenElementRenderer();
    }

    internal abstract class SectionRendererFactory
    {
        internal abstract SectionRenderer CreateSectionRenderer();
    }

    internal abstract class TextureFactory
    {
        internal abstract Texture CreateTexture(string path, int fallbackResolution = 16);
    }
}