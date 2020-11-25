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
            return new OpenGL33.ArrayTexture(path, resolution, useCustomMipmapGeneration, textureUnits);
        }
    }

    internal class BoxRendererFactory : Versions.BoxRendererFactory
    {
        internal override Rendering.BoxRenderer CreateBoxRenderer()
        {
            return new OpenGL33.BoxRenderer();
        }
    }

    internal class OverlayRendererFactory : Versions.OverlayRendererFactory
    {
        internal override Rendering.OverlayRenderer CreateOverlayRenderer()
        {
            return new OpenGL33.OverlayRenderer();
        }
    }

    internal class ScreenFactory : Versions.ScreenFactory
    {
        internal override Rendering.Screen CreateScreen(Client client)
        {
            return new OpenGL33.Screen(client);
        }
    }

    internal class ScreenElementRendererFactory : Versions.ScreenElementRendererFactory
    {
        internal override Rendering.ScreenElementRenderer CreateScreenElementRenderer()
        {
            return new OpenGL33.ScreenElementRenderer();
        }
    }

    internal class SectionRendererFactory : Versions.SectionRendererFactory
    {
        internal override Rendering.SectionRenderer CreateSectionRenderer()
        {
            return new OpenGL33.SectionRenderer();
        }
    }

    internal class TextureFactory : Versions.TextureFactory
    {
        internal override Rendering.Texture CreateTexture(string path, TextureUnit unit, int fallbackResolution = 16)
        {
            return new OpenGL33.Texture(path, unit, fallbackResolution);
        }
    }
}