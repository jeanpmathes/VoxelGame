﻿// <copyright file="ConcreteLiquid.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic.Liquids
{
    public class ConcreteLiquid : BasicLiquid
    {
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

        internal override void RandomUpdate(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            if (!isStatic) return;

            world.SetDefaultLiquid(position);
            Block.Specials.Concrete.Place(world, level, position);
        }
    }
}