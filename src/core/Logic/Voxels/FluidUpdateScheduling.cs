// <copyright file="FluidUpdateScheduling.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2025 Jean Patrick Mathes
//      
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
//     
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//     
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
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
