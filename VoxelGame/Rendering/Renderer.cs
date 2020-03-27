// <copyright file="Renderer.cs" company="VoxelGame">
//     All rights reserved.
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