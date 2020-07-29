// <copyright file="TintedCustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Logic.Interfaces;
using VoxelGame.Utilities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A custom model block that uses tint.
    /// Data bit usage: <c>-cccc</c>
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

        public override uint GetMesh(BlockSide side, uint data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            tint = ((BlockColor)(0b0_1111 & data)).ToTintColor();

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _, out isAnimated);
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Game.World.SetBlock(this, data + 1 & 0b0_1111, x, y, z);
        }
    }
}