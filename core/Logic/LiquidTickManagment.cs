// <copyright file="LiquidTickManagment.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>
using System;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic
{
    public partial class Liquid
    {
        public const int MaxLiquidTicksPerFrameAndChunk = 1024;

        protected void ScheduleTick(int x, int y, int z)
        {
            Chunk? chunk = Game.World.GetChunkOfPosition(x, z);
            chunk?.ScheduleLiquidTick(new LiquidTick(x, y, z, this), Viscosity);
        }

        [Serializable]
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal struct LiquidTick : ITickable
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private readonly int x, y, z;
            private readonly uint target;

            public LiquidTick(int x, int y, int z, Liquid target)
            {
                this.x = x;
                this.y = y;
                this.z = z;

                this.target = target.Id;
            }

            public void Tick()
            {
                Liquid? liquid = Game.World.GetLiquid(x, y, z, out LiquidLevel level, out bool isStatic);

                if (liquid?.Id == target)
                {
                    liquid.ScheduledUpdate(x, y, z, level, isStatic);
                }
            }
        }
    }
}
