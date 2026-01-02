// <copyright file="Plantable.cs" company="VoxelGame">
//     VoxelGame - a voxel-based video game.
//     Copyright (C) 2026 Jean Patrick Mathes
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     The behavior for blocks that can support plant growth.
/// </summary>
public partial class Plantable : BlockBehavior, IBehavior<Plantable, BlockBehavior, Block>
{
    [Constructible]
    private Plantable(Block subject) : base(subject) {}

    /// <summary>
    ///     Whether this block supports full plant growth.
    ///     This means that plants can reach all growth stages and are not limited to only the first few stages.
    /// </summary>
    public ResolvedProperty<Boolean> SupportsFullGrowth { get; } = ResolvedProperty<Boolean>.New<ORing<Void>>(nameof(SupportsFullGrowth));

    [LateInitialization] private partial IEvent<IGrowthAttemptMessage> GrowthAttempt { get; set; }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        GrowthAttempt = registry.RegisterEvent<IGrowthAttemptMessage>(single: true);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        SupportsFullGrowth.Initialize(this);
    }

    /// <summary>
    ///     Try to grow a plant on this block.
    /// </summary>
    /// <param name="world">The world in which the operation takes place.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="fluid">The fluid that is required by the plant.</param>
    /// <param name="level">The amount of fluid required by the plant.</param>
    /// <returns>True if enough fluid was available.</returns>
    public Boolean TryGrow(World world, Vector3i position, Fluid fluid, FluidLevel level)
    {
        if (!GrowthAttempt.HasSubscribers)
            return fluid.TryTakeExact(world, position, level);

        GrowthAttemptMessage growthAttempt = IEventMessage<GrowthAttemptMessage>.Pool.Get();

        growthAttempt.World = world;
        growthAttempt.Position = position;
        growthAttempt.Fluid = fluid;
        growthAttempt.Level = level;
        growthAttempt.CanGrow = false;

        GrowthAttempt.Publish(growthAttempt);

        Boolean canGrow = growthAttempt.CanGrow;

        IEventMessage<GrowthAttemptMessage>.Pool.Return(growthAttempt);

        return canGrow;
    }

    /// <summary>
    ///     Sent when a plant attempts to grow on this block.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IGrowthAttemptMessage
    {
        /// <summary>
        ///     The world in which the placement was completed.
        /// </summary>
        World World { get; }

        /// <summary>
        ///     The position at which the block was placed.
        /// </summary>
        Vector3i Position { get; }

        /// <summary>
        ///     The fluid that is required by the plant.
        /// </summary>
        Fluid Fluid { get; }

        /// <summary>
        ///     The amount of fluid required by the plant.
        /// </summary>
        FluidLevel Level { get; }

        /// <summary>
        ///     Whether the plant can grow on this block.
        /// </summary>
        Boolean CanGrow { get; }

        /// <summary>
        ///     Mark that the growth attempt succeeded.
        ///     A caller calling this must remove the required fluid from the world.
        /// </summary>
        void MarkAsSuccessful();
    }

    private sealed partial record GrowthAttemptMessage
    {
        public void MarkAsSuccessful()
        {
            CanGrow = true;
        }
    }
}
