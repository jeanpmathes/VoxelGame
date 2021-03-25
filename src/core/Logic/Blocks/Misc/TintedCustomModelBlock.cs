// <copyright file="TintedCustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A custom model block that uses tint.
    /// Data bit usage: <c>-ccccc</c>
    /// </summary>
    // c = color
    public class TintedCustomModelBlock : CustomModelBlock, IFlammable
    {
        public TintedCustomModelBlock(string name, string namedId, string modelName, Physics.BoundingBox boundingBox, bool isSolid = true) :
            base(
                name,
                namedId,
                modelName,
                boundingBox,
                isSolid,
                isInteractable: true)
        {
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(((BlockColor)(0b01_1111 & info.Data)).ToTintColor());
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(this, data + 1 & 0b01_1111, x, y, z);
        }
    }
}