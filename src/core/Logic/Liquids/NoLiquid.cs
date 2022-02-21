// <copyright file="NoLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Liquids
{
    /// <summary>
    ///     This liquid represents the absence of liquids.
    /// </summary>
    public class NoLiquid : Liquid
    {
        /// <summary>
        ///     Creates a new <see cref="NoLiquid" />.
        /// </summary>
        /// <param name="name">The name of the liquid.</param>
        /// <param name="namedId">The named ID.</param>
        public NoLiquid(string name, string namedId) :
            base(
                name,
                namedId,
                AirDensity,
                viscosity: 1,
                checkContact: false,
                receiveContact: false,
                RenderType.NotRendered) {}

        /// <inheritdoc />
        public override LiquidMeshData GetMesh(LiquidMeshInfo info)
        {
            return LiquidMeshData.Empty;
        }

        /// <inheritdoc />
        protected override void ScheduledUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic) {}
    }
}
