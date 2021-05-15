// <copyright file="ConcreteBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using VoxelGame.Core.Entities;
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Physics;
using VoxelGame.Core.Utilities;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A block that can have different heights and colors. The heights correspond to liquid heights.
    /// Data bit usage: <c>ccchhh</c>
    /// </summary>
    // c = color
    // h = height
    public class ConcreteBlock : Block, IHeightVariable, IConnectable
    {
        private readonly TextureLayout layout;
        private int[] textures = null!;

        internal ConcreteBlock(string name, string namedId, TextureLayout layout) :
            base(
                name,
                namedId,
                isFull: false,
                isOpaque: false,
                renderFaceAtNonOpaques: false,
                isSolid: true,
                receiveCollisions: false,
                isTrigger: false,
                isReplaceable: false,
                isInteractable: true,
                BoundingBox.Block,
                TargetBuffer.VaryingHeight)
        {
            this.layout = layout;
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
            return BlockMeshData.VaryingHeight(textures[(int)info.Side], color.ToTintColor());
        }

        protected override void DoPlace(int x, int y, int z, PhysicsEntity? entity)
        {
            Game.World.SetBlock(this, Encode(BlockColor.Default, IHeightVariable.MaximumHeight), x, y, z);
        }

        public void Place(LiquidLevel level, int x, int y, int z)
        {
            if (Place(x, y, z))
            {
                Game.World.SetBlock(this, Encode(BlockColor.Default, level.GetBlockHeight()), x, y, z);
            }
        }

        protected override void EntityInteract(PhysicsEntity entity, int x, int y, int z, uint data)
        {
            Decode(data, out BlockColor color, out int height);
            var next = (BlockColor)((int)color + 1);
            Game.World.SetBlock(this, Encode(next, height), x, y, z);
        }

        private static uint Encode(BlockColor color, int height)
        {
            var val = 0;
            val |= ((int)color << 3) & 0b11_1000;
            val |= (height / 2) & 0b00_0111;
            return (uint)val;
        }

        private static void Decode(uint data, out BlockColor color, out int height)
        {
            color = (BlockColor)((data & 0b11_1000) >> 3);
            height = ((int)(data & 0b00_0111) * 2) + 1;
        }

        public int GetHeight(uint data)
        {
            Decode(data, out _, out int height);
            return height;
        }

        public bool IsConnectable(BlockSide side, int x, int y, int z)
        {
            Game.World.GetBlock(x, y, z, out uint data);
            return GetHeight(data) == IHeightVariable.MaximumHeight;
        }
    }
}