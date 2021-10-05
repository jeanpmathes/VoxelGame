﻿// <copyright file="LiquidTickManagement.cs" company="VoxelGame">
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
        public const int MaxLiquidTicksPerFrameAndChunk = 1024;

        /// <summary>
        ///     Schedules a tick according to the viscosity.
        /// </summary>
        protected void ScheduleTick(World world, Vector3i position)
        {
            Chunk? chunk = world.GetChunkOfPosition(position);
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
                Liquid? liquid = world.GetLiquid((x, y, z), out LiquidLevel level, out bool isStatic);

                if (liquid?.Id == target) liquid.ScheduledUpdate(world, (x, y, z), level, isStatic);
            }
        }
    }
}