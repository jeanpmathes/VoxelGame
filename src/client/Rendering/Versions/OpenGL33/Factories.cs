// <copyright file="Factories.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Graphics.OpenGL4;

namespace VoxelGame.Client.Rendering.Versions.OpenGL33
{
    internal class ArrayTextureFactory : Versions.ArrayTextureFactory
    {
        internal override Rendering.ArrayTexture CreateArrayTexture(string path, int resolution, bool useCustomMipmapGeneration, params TextureUnit[] textureUnits)
        {
            return new ArrayTexture(path, resolution, useCustomMipmapGeneration, textureUnits);
        }
    }

    internal class BoxRendererFactory : Versions.BoxRendererFactory
    {
        internal override Rendering.BoxRenderer CreateBoxRenderer()
        {
            return new BoxRenderer();
        }
    }

    internal class ScreenFactory : Versions.ScreenFactory
    {
        internal override Rendering.Screen CreateScreen(Client client)
        {
            return new Screen(client);
        }
    }

    internal class ScreenElementRendererFactory : Versions.ScreenElementRendererFactory
    {
        internal override Rendering.ScreenElementRenderer CreateScreenElementRenderer()
        {
            return new ScreenElementRenderer();
        }
    }

    internal class SectionRendererFactory : Versions.SectionRendererFactory
    {
        internal override Rendering.SectionRenderer CreateSectionRenderer()
        {
            return new SectionRenderer();
        }
    }

    internal class TextureFactory : Versions.TextureFactory
    {
        internal override Rendering.Texture CreateTexture(string path, int fallbackResolution = 16)
        {
            return new Texture(path, fallbackResolution);
        }
    }
}