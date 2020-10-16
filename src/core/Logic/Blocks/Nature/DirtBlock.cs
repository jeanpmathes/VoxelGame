// <copyright file="DirtBlock.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Logic.Interfaces;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Blocks
{
    /// <summary>
    /// A simple block which allows the spread of grass.
    /// Data bit usage: <c>------</c>
    /// </summary>
    public class DirtBlock : BasicBlock, IPlantable, IGrassSpreadable, IFillable
    {
        private protected int[][] wetTextureIndices = null!;

        private protected TextureLayout wet;

        public DirtBlock(string name, string namedId, TextureLayout normal, TextureLayout wet) :
            base(
                name,
                namedId,
                layout: normal,
                isOpaque: true,
                renderFaceAtNonOpaques: true,
                isSolid: true,
                recieveCollisions: false,
                isTrigger: false,
                isInteractable: false)
        {
            this.wet = wet;
        }

        protected override void Setup()
        {
            base.Setup();

            wetTextureIndices = wet.GetTexIndexArrays();
        }

        public override uint GetMesh(BlockSide side, uint data, Liquid liquid, out float[] vertices, out int[] textureIndices, out uint[] indices, out TintColor tint, out bool isAnimated)
        {
            uint verts = base.GetMesh(side, data, liquid, out vertices, out textureIndices, out indices, out tint, out isAnimated);

            if (liquid.Direction > 0) textureIndices = wetTextureIndices[(int)side];

            return verts;
        }

        internal override void RandomUpdate(int x, int y, int z, uint data)
        {
            Liquid? liquid = Game.World.GetLiquid(x, y, z, out LiquidLevel level, out _);

            if (liquid == Liquid.Water && level == LiquidLevel.Eight)
            {
                Game.World.SetBlock(Block.Mud, 0, x, y, z);
            }
        }

        public virtual bool IsFillable(int x, int y, int z, Liquid liquid)
        {
            return liquid.Viscosity < 200;
        }
    }
}