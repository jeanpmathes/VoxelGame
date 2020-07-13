// <copyright file="TintedCustomModelBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Entities;
using VoxelGame.Utilities;
using VoxelGame.Visuals;

namespace VoxelGame.Logic.Blocks
{
    /// <summary>
    /// A custom model block that uses tint.
    /// Data bit usage: <c>-cccc</c>
    /// </summary>
    // c = color
    public class TintedCustomModelBlock : CustomModelBlock
    {
        public TintedCustomModelBlock(string name, string namedId, string modelName, bool isSolid, Physics.BoundingBox boundingBox) :
            base(
                name,
                namedId,
                modelName,
                isSolid,
                boundingBox)
        {
        }

        public override uint GetMesh(BlockSide side, byte data, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint)
        {
            tint = ((BlockColor)(0b0_1111 & data)).ToTintColor();

            return base.GetMesh(side, data, out vertices, out textureIndices, out indices, out _);
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, byte data)
        {
            Game.World.SetBlock(this, (byte)(data + 1 & 0b0_1111), x, y, z);
        }
    }
}