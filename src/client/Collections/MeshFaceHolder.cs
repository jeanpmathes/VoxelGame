// <copyright file="MeshFaceHolder.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic;

namespace VoxelGame.Client.Collections
{
    /// <summary>
    ///     Holds and combines mesh faces, that then can be used to create a final mesh.
    /// </summary>
    public class MeshFaceHolder
    {
        private protected readonly BlockSide side;

        /// <summary>
        ///     Create a new <see cref="MeshFaceHolder" />
        /// </summary>
        /// <param name="side">The block side to target.</param>
        protected MeshFaceHolder(BlockSide side)
        {
            this.side = side;
        }

        /// <summary>
        ///     Extract the layer and row indices from the given block position.
        /// </summary>
        /// <param name="pos">The block position.</param>
        /// <param name="layer">The extracted layer index.</param>
        /// <param name="row">The extracted row index.</param>
        /// <param name="position">The extracted position in the row.</param>
        protected void ExtractIndices(Vector3i pos, out int layer, out int row, out int position)
        {
            switch (side)
            {
                case BlockSide.Front:
                case BlockSide.Back:
                    layer = pos.Z;
                    row = pos.X;
                    position = pos.Y;

                    break;

                case BlockSide.Left:
                case BlockSide.Right:
                    layer = pos.X;
                    row = pos.Y;
                    position = pos.Z;

                    break;

                case BlockSide.Bottom:
                case BlockSide.Top:
                    layer = pos.Y;
                    row = pos.X;
                    position = pos.Z;

                    break;

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
