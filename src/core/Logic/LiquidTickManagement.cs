// <copyright file="LiquidTickManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic
{
    public partial class Liquid
    {
        /// <summary>
        ///     The maximum amount of liquid ticks per frame.
        /// </summary>
        internal const int MaxLiquidTicksPerFrameAndChunk = 1024;

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
        internal struct LiquidTick : ITickable, IEquatable<LiquidTick>
        {
            private readonly int x;
            private readonly int y;
            private readonly int z;

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
                LiquidInstance? potentialLiquid = world.GetLiquid((x, y, z));

                if (potentialLiquid is not {} liquid) return;

                if (liquid.Liquid.Id == target)
                    liquid.Liquid.ScheduledUpdate(world, (x, y, z), liquid.Level, liquid.IsStatic);
            }

            public bool Equals(LiquidTick other)
            {
                return x == other.x && y == other.y && z == other.z && target == other.target;
            }

            public override bool Equals(object? obj)
            {
                return obj is LiquidTick other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(x, y, z, target);
            }

            public static bool operator ==(LiquidTick left, LiquidTick right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(LiquidTick left, LiquidTick right)
            {
                return !left.Equals(right);
            }
        }
    }
}
