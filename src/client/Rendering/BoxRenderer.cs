// <copyright file="BoxRenderer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Physics;

namespace VoxelGame.Client.Rendering
{
    /// <summary>
    /// A renderer that renders instances of the <see cref="BoundingBox"/> struct.
    /// </summary>
    public abstract class BoxRenderer : Renderer
    {
        public abstract void SetBoundingBox(BoundingBox boundingBox);
    }
}