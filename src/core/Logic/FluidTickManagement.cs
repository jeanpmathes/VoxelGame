// <copyright file="FluidTickManagement.cs" company="VoxelGame">
//     MIT License
//	   For full license see the repository.
// </copyright>
// <author>pershingthesecond</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;

namespace VoxelGame.Core.Logic;

public partial class Fluid
{
    /// <summary>
    ///     The maximum amount of fluid ticks per frame.
    /// </summary>
    internal const int MaxFluidTicksPerFrameAndChunk = 1024;

    /// <summary>
    ///     Schedules a tick according to the viscosity.
    /// </summary>
    protected void ScheduleTick(World world, Vector3i position)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleFluidTick(new FluidTick(position, this), Viscosity);
    }

    /// <summary>
    ///     Will schedule a tick for a fluid according to the viscosity.
    /// </summary>
    internal void TickSoon(World world, Vector3i position, bool isStatic)
    {
        if (!isStatic || this == None) return;

        world.ModifyFluid(isStatic: false, position);
        ScheduleTick(world, position);
    }

    /// <summary>
    ///     Will schedule a tick for a fluid in the next possible update.
    /// </summary>
    internal void TickNow(World world, Vector3i position, FluidLevel level, bool isStatic)
    {
        if (this == None) return;

        ScheduledUpdate(world, position, level, isStatic);
    }

    [Serializable]
    internal struct FluidTick : ITickable, IEquatable<FluidTick>
    {
        private readonly int x;
        private readonly int y;
        private readonly int z;

        private readonly uint target;

        public FluidTick(Vector3i position, Fluid target)
        {
            x = position.X;
            y = position.Y;
            z = position.Z;

            this.target = target.Id;
        }

        public void Tick(World world)
        {
            FluidInstance? potentialFluid = world.GetFluid((x, y, z));

            if (potentialFluid is not {} fluid) return;

            if (fluid.Fluid.Id == target)
                fluid.Fluid.ScheduledUpdate(world, (x, y, z), fluid.Level, fluid.IsStatic);
        }

        public bool Equals(FluidTick other)
        {
            return x == other.x && y == other.y && z == other.z && target == other.target;
        }

        public override bool Equals(object? obj)
        {
            return obj is FluidTick other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y, z, target);
        }

        public static bool operator ==(FluidTick left, FluidTick right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FluidTick left, FluidTick right)
        {
            return !left.Equals(right);
        }
    }
}
