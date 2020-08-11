// <copyright file="NoLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Liquids
{
    /// <summary>
    /// This liquid represents the absence of liquids.
    /// </summary>
    public class NoLiquid : Liquid
    {
        public NoLiquid(string name, string namedId) :
            base(
                name,
                namedId,
                isRendered: false)
        {
        }

        public override uint GetMesh(BlockSide side, LiquidLevel level, bool isStatic, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            vertices = Array.Empty<float>();
            textureIndices = Array.Empty<int>();
            indices = Array.Empty<uint>();

            tint = TintColor.None;

            return 4;
        }
    }
}