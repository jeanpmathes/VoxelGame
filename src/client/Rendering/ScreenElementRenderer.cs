// <copyright file="OverlayRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenToolkit.Mathematics;

namespace VoxelGame.Client.Rendering
{
    public abstract class ScreenElementRenderer : Renderer
    {
        public abstract void SetTexture(Texture texture);

        public abstract void SetColor(Vector3 color);
    }
}