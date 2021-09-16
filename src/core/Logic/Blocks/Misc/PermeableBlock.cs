// <copyright file="PermeableBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A solid and full block that allows water flow through it. The become darker in liquids.
    ///     Data bit usage: <c>------</c>
    /// </summary>
    public class PermeableBlock : BasicBlock, IFillable
    {
        internal PermeableBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                layout,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isInteractable: false) {}

        public virtual bool AllowInflow(World world, Vector3i position, BlockSide side, Liquid liquid)
        {
            return liquid.Viscosity < 100;
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(info.Liquid.Direction > 0 ? TintColor.LightGray : TintColor.None);
        }
    }
}