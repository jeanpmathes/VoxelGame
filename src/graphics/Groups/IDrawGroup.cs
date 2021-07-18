// <copyright file="IDrawGroup.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Graphics.Groups
{
    /// <summary>
    /// An interface with common draw group operations, hiding the actual OpenGL internals.
    /// </summary>
    public interface IDrawGroup
    {
        public bool IsFilled { get; }

        public void BindVertexArray();

        public void Draw();
    }
}