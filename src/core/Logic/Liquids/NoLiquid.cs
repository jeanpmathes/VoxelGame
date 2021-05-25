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
                density: 0f,
                viscosity: 1,
                checkContact: false,
                receiveContact: false,
                RenderType.NotRendered)
        {
        }

        public override LiquidMeshData GetMesh(LiquidMeshInfo info)
        {
            return LiquidMeshData.Empty;
        }

        protected override void ScheduledUpdate(World world, int x, int y, int z, LiquidLevel level, bool isStatic)
        {
        }
    }
}