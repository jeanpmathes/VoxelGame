// <copyright file="TargetBuffer.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

namespace VoxelGame.Core.Visuals
{
    /// <summary>
    ///     The meshing and rendering buffer of a block.
    /// </summary>
    public enum TargetBuffer
    {
        /// <summary>
        /// For blocks that are not rendered.
        /// </summary>
        NotRendered,

        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.Front"/> to <see cref="Logic.BlockSide.Top"/>. Blocks that target this buffer have to be full.
        /// GetMesh has to return exactly 4 vertices that make up one face. Only UVs can vary between the vertices.
        /// Indices are ignored, so the counter-clockwise order <c>{(0, 2, 1), (0, 3, 2)}</c> has to be followed, starting at the bottom left.
        /// The UVs may only be rotated counter-clockwise once.
        /// </summary>
        Simple,

        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.All"/>.
        /// </summary>
        Complex,

        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.Front"/> to <see cref="Logic.BlockSide.Top"/>.
        /// Blocks that target this buffer have to implement <see cref="Logic.Interfaces.IHeightVariable"/>.
        /// </summary>
        VaryingHeight,

        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.All"/> and conform to the cross plant requirements.
        /// </summary>
        CrossPlant,

        /// <summary>
        /// Blocks have to accept <see cref="Logic.BlockSide.All"/> and conform to the crop plant requirements.
        /// </summary>
        CropPlant
    }
}
