// <copyright file="LiquidContactManager.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using OpenToolkit.Mathematics;

namespace VoxelGame.Core.Logic
{
    public static class LiquidContactManager
    {
        public static bool HandleContact(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            switch ((a.NamedId, b.NamedId))
            {
                default: return DensitySwap(a, posA, levelA, b, posB, levelB, isStaticB);
            }
        }

        private static bool DensitySwap(Liquid a, Vector3i posA, LiquidLevel levelA, Liquid b, Vector3i posB, LiquidLevel levelB, bool isStaticB)
        {
            if ((posA.Y <= posB.Y || !(a.Density > b.Density)) &&
                (posA.Y >= posB.Y || !(a.Density < b.Density))) return false;

            Game.World.SetLiquid(a, levelA, false, posB.X, posB.Y, posB.Z);

            a.TickSoon(posB.X, posB.Y, posB.Z, isStaticB);

            Game.World.SetLiquid(b, levelB, false, posA.X, posA.Y, posA.Z);

            b.TickSoon(posA.X, posA.Y, posA.Z, true);

            return true;
        }
    }
}