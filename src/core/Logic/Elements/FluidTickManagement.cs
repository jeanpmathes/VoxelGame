// <copyright file="FluidTickManagement.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Logic.Elements;

public partial class Fluid
{
    /// <summary>
    ///     Schedules a tick according to the viscosity.
    /// </summary>
    protected void ScheduleTick(World world, Vector3i position)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleFluidTick(new FluidTick(position, this), (UInt32) Viscosity);
    }

    /// <summary>
    ///     Will schedule a tick for a fluid according to the viscosity.
    /// </summary>
    internal void TickSoon(World world, Vector3i position, Boolean isStatic)
    {
        if (!isStatic || this == Fluids.Instance.None) return;

        world.ModifyFluid(isStatic: false, position);
        ScheduleTick(world, position);
    }

    /// <summary>
    ///     Will tick a fluid as soon as possible, meaning now.
    /// </summary>
    internal void TickNow(World world, Vector3i position, FluidInstance instance)
    {
        if (this == Fluids.Instance.None) return;

        ScheduledUpdate(world, position, instance);
    }

    internal struct FluidTick(Vector3i position, Fluid target) : ITickable, IEquatable<FluidTick>
    {
        private Int32 x = position.X;
        private Int32 y = position.Y;
        private Int32 z = position.Z;

        private UInt32 target = target.ID;

        public void Tick(World world)
        {
            FluidInstance? potentialFluid = world.GetFluid((x, y, z));

            if (potentialFluid is not {} fluid) return;

            if (fluid.Fluid.ID == target)
                fluid.Fluid.ScheduledUpdate(world, (x, y, z), fluid);
        }

        public void Serialize(Serializer serializer)
        {
            serializer.Serialize(ref x);
            serializer.Serialize(ref y);
            serializer.Serialize(ref z);
            serializer.Serialize(ref target);
        }

        public Boolean Equals(FluidTick other)
        {
            return x == other.x && y == other.y && z == other.z && target == other.target;
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is FluidTick other && Equals(other);
        }

#pragma warning disable S2328
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(x, y, z, target);
        }
#pragma warning restore S2328

        public static Boolean operator ==(FluidTick left, FluidTick right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(FluidTick left, FluidTick right)
        {
            return !left.Equals(right);
        }
    }
}
