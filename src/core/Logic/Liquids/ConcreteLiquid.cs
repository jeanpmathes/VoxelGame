// <copyright file="ConcreteLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Liquids
{
    /// <summary>
    ///     A concrete-like liquid that can harden to concrete blocks.
    /// </summary>
    public class ConcreteLiquid : BasicLiquid
    {
        /// <summary>
        ///     Create a new <see cref="ConcreteLiquid" />.
        /// </summary>
        /// <param name="name">The name of the liquid.</param>
        /// <param name="namedId">The named ID of the liquid.</param>
        /// <param name="density">The density of the liquid.</param>
        /// <param name="viscosity">The viscosity of the liquid.</param>
        /// <param name="movingLayout">The texture layout when this liquid is moving.</param>
        /// <param name="staticLayout">The texture layout when this liquid is static.</param>
        public ConcreteLiquid(string name, string namedId, float density, int viscosity, TextureLayout movingLayout,
            TextureLayout staticLayout) :
            base(
                name,
                namedId,
                density,
                viscosity,
                neutralTint: false,
                movingLayout,
                staticLayout) {}

        /// <inheritdoc />
        internal override void RandomUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            if (!isStatic) return;

            world.SetDefaultLiquid(position);
            Block.Specials.Concrete.Place(world, level, position);
        }
    }
}
