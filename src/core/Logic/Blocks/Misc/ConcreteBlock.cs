// <copyright file="ConcreteBlock.cs" company="VoxelGame">
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
    ///     A block that can have different heights and colors. The heights correspond to liquid heights.
    ///     Data bit usage: <c>ccchhh</c>
    /// </summary>
    // c: color
    // h: height
    public class ConcreteBlock : Block, IHeightVariable, IWideConnectable, IThinConnectable
    {
        private readonly TextureLayout layout;
        private int[] textures = null!;

        internal ConcreteBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                BlockFlags.Functional,
                BoundingBox.Block,
                TargetBuffer.VaryingHeight)
        {
            this.layout = layout;
        }

        public int GetHeight(uint data)
        {
            Decode(data, out _, out int height);

            return height;
        }

        public bool IsConnectable(World world, BlockSide side, Vector3i position)
        {
            BlockInstance? block = world.GetBlock(position);

            return block != null && GetHeight(block.Data) == IHeightVariable.MaximumHeight;
        }

        protected override void Setup(ITextureIndexProvider indexProvider)
        {
            textures = layout.GetTexIndexArray();
        }

        protected override BoundingBox GetBoundingBox(uint data)
        {
            Decode(data, out _, out int height);

            return BoundingBox.BlockWithHeight(height);
        }

        public override BlockMeshData GetMesh(BlockMeshInfo info)
        {
            Decode(info.Data, out BlockColor color, out _);

            return BlockMeshData.VaryingHeight(textures[(int) info.Side], color.ToTintColor());
        }

        protected override void DoPlace(World world, Vector3i position, PhysicsEntity? entity)
        {
            world.SetBlock(this.AsInstance(Encode(BlockColor.Default, IHeightVariable.MaximumHeight)), position);
        }

        public void Place(World world, LiquidLevel level, Vector3i position)
        {
            if (base.Place(world, position))
                world.SetBlock(this.AsInstance(Encode(BlockColor.Default, level.GetBlockHeight())), position);
        }

        protected override void EntityInteract(PhysicsEntity entity, Vector3i position, uint data)
        {
            Decode(data, out BlockColor color, out int height);
            var next = (BlockColor) ((int) color + 1);
            entity.World.SetBlock(this.AsInstance(Encode(next, height)), position);
        }

        private static uint Encode(BlockColor color, int height)
        {
            var val = 0;
            val |= ((int) color << 3) & 0b11_1000;
            val |= (height / 2) & 0b00_0111;

            return (uint) val;
        }

        private static void Decode(uint data, out BlockColor color, out int height)
        {
            color = (BlockColor) ((data & 0b11_1000) >> 3);
            height = (int) (data & 0b00_0111) * 2 + 1;
        }
    }
}