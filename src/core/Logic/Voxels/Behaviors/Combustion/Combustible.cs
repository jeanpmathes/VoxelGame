// <copyright file="Combustible.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Utilities;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;

/// <summary>
///     Makes a block able to be burned.
/// </summary>
public partial class Combustible : BlockBehavior, IBehavior<Combustible, BlockBehavior, Block>
{
    [Constructible]
    private Combustible(Block subject) : base(subject)
    {
        BurnedState = Aspect<State?, (World, Vector3i, State, Block)>.New<Exclusive<State?, (World, Vector3i, State, Block)>>(nameof(BurnedState), this);
    }

    [LateInitialization] private partial IEvent<IBurnMessage> Burn { get; set; }

    /// <summary>
    ///     The chance that, on combustion, the block is completely destroyed instead of changing state into the
    ///     <see cref="BurnedState" />.
    /// </summary>
    public ResolvedProperty<Chance> CompleteDestructionChance { get; } = ResolvedProperty<Chance>.New<Exclusive<Chance, Void>>(nameof(CompleteDestructionChance), Chance.Impossible);

    /// <summary>
    ///     Determine the state a block should change into after burning.
    /// </summary>
    public Aspect<State?, (World world, Vector3i position, State state, Block fire)> BurnedState { get; }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        Burn = registry.RegisterEvent<IBurnMessage>();
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        CompleteDestructionChance.Initialize(this);
    }

    /// <summary>
    ///     Burn a block at a given position.
    ///     The block can either be destroyed, or change into a different state or block.
    /// </summary>
    /// <param name="world">The world this block is in.</param>
    /// <param name="position">The position of the block.</param>
    /// <param name="fire">The fire block that caused the burning.</param>
    /// <returns><c>true</c> if the block was destroyed, <c>false</c> if not.</returns>
    public Boolean DoBurn(World world, Vector3i position, Block fire)
    {
        State? state = world.GetBlock(position);

        if (state == null)
            return false;

        State? burnedState = BurnedState.GetValue(original: null, (world, position, state.Value, fire));

        if (!Burn.HasSubscribers)
        {
            if (burnedState == null || NumberGenerator.GetPositionDependentOutcome(position, CompleteDestructionChance.Get()))
                return Subject.Destroy(world, position);

            world.SetBlock(burnedState.Value, position);

            fire.Place(world, position.Above());

            foreach (Orientation orientation in Orientations.All)
                Fire.PlaceOriented(world, position.Offset(orientation), orientation.Opposite(), fire);

            return false;
        }

        BurnMessage burn = IEventMessage<BurnMessage>.Pool.Get();

        burn.World = world;
        burn.Position = position;
        burn.Fire = fire;
        burn.Burned = false;

        Burn.Publish(burn);

        Boolean burned = burn.Burned;

        IEventMessage<BurnMessage>.Pool.Return(burn);

        if (burned)
            return true;

        if (burnedState == null)
            return false;

        world.SetBlock(burnedState.Value, position);

        return false;
    }

    /// <summary>
    ///     Sent when a block is burned.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IBurnMessage
    {
        /// <summary>
        ///     The world the block is in.
        /// </summary>
        World World { get; }

        /// <summary>
        ///     The position of the block that is burning.
        /// </summary>
        Vector3i Position { get; }

        /// <summary>
        ///     The fire block that caused the burning.
        /// </summary>
        Block Fire { get; }

        /// <summary>
        ///     Whether the block has been destroyed by the burn operation.
        /// </summary>
        Boolean Burned { get; }

        /// <summary>
        ///     Set that the block has been burned (destroyed or changed).
        ///     This will set <see cref="Burned" /> to <c>true</c>.
        /// </summary>
        void Burn();
    }

    private sealed partial record BurnMessage
    {
        public void Burn()
        {
            Burned = true;
        }
    }
}
