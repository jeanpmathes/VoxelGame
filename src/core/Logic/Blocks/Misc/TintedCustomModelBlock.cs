// <copyright file="TintedCustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    ///     A custom model block that uses tint.
    ///     Data bit usage: <c>-ccccc</c>
    /// </summary>
    // c: color
    public class TintedCustomModelBlock : CustomModelBlock, IFlammable
    {
        internal TintedCustomModelBlock(string name, string namedId, BlockFlags flags, string modelName,
            BoundingBox boundingBox) :
            base(
                name,
                namedId,
                flags with { IsInteractable = true },
                modelName,
                boundingBox) {}

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            return base.GetMesh(info).Modified(((BlockColor) (0b01_1111 & info.Data)).ToTintColor());
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            entity.World.SetBlock(this.AsInstance((data + 1) & 0b01_1111), position);
        }
    }
}