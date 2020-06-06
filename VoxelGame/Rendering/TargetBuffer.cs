// <copyright file="TargetBuffer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
namespace VoxelGame.Rendering
{
    public enum TargetBuffer
    {
        /// <summary>
        /// For blocks that are not rendered.
        /// </summary>
        NotRendered,
        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.Front"/> to <see cref="Logic.BlockSide.Top"/>. Blocks that target this buffer have to be full.
        /// GetMesh has to return 6 vertices that make up one face, indices are ignored.
        /// </summary>
        Simple,
        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.All"/>.
        /// </summary>
        Complex
    }
}
