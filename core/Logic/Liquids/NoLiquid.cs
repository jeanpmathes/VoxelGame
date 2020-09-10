// <copyright file="NoLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
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
                0f,
                1,
                isRendered: false)
        {
        }

        public override void GetMesh(LiquidLevel level, BlockSide side, bool isStatic, out int textureIndex, out TintColor tint)
        {
            textureIndex = 0;
            tint = TintColor.None;
        }

        protected override void ScheduledUpdate(int x, int y, int z, LiquidLevel level, bool isStatic)
        {
        }
    }
}