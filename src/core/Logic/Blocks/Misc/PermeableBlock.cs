// <copyright file="PermeableBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A solid and full block that allows water flow through it. The become darker in liquids.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class PermeableBlock : BasicBlock, IFillable
    {
        public PermeableBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            tint = (liquid.Direction > 0) ? TintColor.LightGray : TintColor.None;

            return base.GetMesh(side, data, liquid, out vertices, out textureIndices, out indices, out _, out isAnimated);
        }

        public virtual bool AllowInflow(int x, int y, int z, BlockSide side, Liquid liquid)
        {
            return liquid.Viscosity < 200;
        }
    }
}