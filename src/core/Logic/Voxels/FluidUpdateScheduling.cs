// <copyright file="FluidUpdateScheduling.cs" company="VoxelGame">
//     MIT License
//     For full license see the repository.
// </copyright>
// <author>jeanpmathes</author>

using System;
using OpenTK.Mathematics;
using VoxelGame.Core.Collections;
using VoxelGame.Core.Logic.Chunks;
using VoxelGame.Core.Serialization;

namespace VoxelGame.Core.Logic.Voxels;

public partial class Fluid
{
    /// <summary>
    ///     Schedules an update according to the viscosity.
    /// </summary>
    protected void ScheduleUpdate(World world, Vector3i position)
    {
        Chunk? chunk = world.GetActiveChunk(position);
        chunk?.ScheduleFluidUpdate(new FluidUpdate(position, this), Viscosity.ToUpdateDelay());
    }

    /// <summary>
    ///     Will schedule an update for a fluid according to the viscosity.
    /// </summary>
    internal void UpdateSoon(World world, Vector3i position, Boolean isStatic)
    {
        if (!isStatic || this == Fluids.Instance.None) return;

        world.ModifyFluid(isStatic: false, position);
        ScheduleUpdate(world, position);
    }

    /// <summary>
    ///     Will update a fluid as soon as possible, meaning now.
    /// </summary>
    internal void UpdateNow(World world, Vector3i position, FluidInstance instance)
    {
        if (this == Fluids.Instance.None) return;

        ScheduledUpdate(world, position, instance);
    }

    internal struct FluidUpdate(Vector3i position, Fluid target) : IUpdateable, IEquatable<FluidUpdate>
    {
        private Int32 x = position.X;
        private Int32 y = position.Y;
        private Int32 z = position.Z;

        private UInt32 target = target.ID;

        public void Update(World world)
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

        public Boolean Equals(FluidUpdate other)
        {
            return x == other.x && y == other.y && z == other.z && target == other.target;
        }

        public override Boolean Equals(Object? obj)
        {
            return obj is FluidUpdate other && Equals(other);
        }

#pragma warning disable S2328
        public override Int32 GetHashCode()
        {
            return HashCode.Combine(x, y, z, target);
        }
#pragma warning restore S2328

        public static Boolean operator ==(FluidUpdate left, FluidUpdate right)
        {
            return left.Equals(right);
        }

        public static Boolean operator !=(FluidUpdate left, FluidUpdate right)
        {
            return !left.Equals(right);
        }
    }
}
