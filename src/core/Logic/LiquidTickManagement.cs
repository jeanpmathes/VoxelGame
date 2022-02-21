// <copyright file="LiquidTickManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenToolkit.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic
{
    public partial class Liquid
    {
        /// <summary>
        ///     The maximum amount of liquid ticks per frame.
        /// </summary>
        public const int MaxLiquidTicksPerFrameAndChunk = 1024;

        /// <summary>
        ///     Schedules a tick according to the viscosity.
        /// </summary>
        protected void ScheduleTick(World world, Vector3i position)
        {
            Chunk? chunk = world.GetChunkWithPosition(position);
            chunk?.ScheduleLiquidTick(new LiquidTick(position, this), Viscosity);
        }

        /// <summary>
        ///     Will schedule a tick for a liquid according to the viscosity.
        /// </summary>
        internal void TickSoon(World world, Vector3i position, bool isStatic)
        {
            if (!isStatic || this == None) return;

            world.ModifyLiquid(isStatic: false, position);
            ScheduleTick(world, position);
        }

        /// <summary>
        ///     Will schedule a tick for a liquid in the next possible update.
        /// </summary>
        internal void TickNow(World world, Vector3i position, LiquidLevel level, bool isStatic)
        {
            if (this == None) return;

            ScheduledUpdate(world, position, level, isStatic);
        }

        [Serializable]
#pragma warning disable CA1815 // Override equals and operator equals on value types
        internal struct LiquidTick : ITickable
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            private readonly int x, y, z;
            private readonly uint target;

            public LiquidTick(Vector3i position, Liquid target)
            {
                x = position.X;
                y = position.Y;
                z = position.Z;

                this.target = target.Id;
            }

            public void Tick(World world)
            {
                LiquidInstance? liquid = world.GetLiquid((x, y, z));

                if (liquid?.Liquid.Id == target)
                    liquid.Liquid.ScheduledUpdate(world, (x, y, z), liquid.Level, liquid.IsStatic);
            }
        }
    }
}
