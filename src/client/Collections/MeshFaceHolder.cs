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
    public abstract class MeshFaceHolder
    {
        private protected readonly BlockSide side;

        protected MeshFaceHolder(BlockSide side)
        {
            this.side = side;
        }

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