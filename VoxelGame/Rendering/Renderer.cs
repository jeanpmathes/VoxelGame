// <copyright file="Renderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using OpenTK;

namespace VoxelGame.Rendering
{
    public abstract class Renderer
    {
        public abstract void Draw(Vector3 position);
    }
}