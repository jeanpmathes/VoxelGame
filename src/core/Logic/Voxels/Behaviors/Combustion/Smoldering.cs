// <copyright file="Smoldering.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;

/// <summary>
///     Adds a smoldering state to a block, showing a glowing ember texture that will eventually cool down.
/// </summary>
public partial class Smoldering : BlockBehavior, IBehavior<Smoldering, BlockBehavior, Block>
{
    [Constructible]
    private Smoldering(Block subject) : base(subject)
    {
        subject.Require<Meshed>().IsAnimated.ContributeFunction(IsAnimated);
        subject.Require<CubeTextured>().ActiveTexture.ContributeFunction(GetTexture);
    }

    [LateInitialization] private partial IAttributeData<Boolean> HasEmbers { get; set; }

    /// <summary>
    ///     The texture layout to use while embers are glowing.
    /// </summary>
    public ResolvedProperty<TextureLayout> EmberTexture { get; } = ResolvedProperty<TextureLayout>.New<Exclusive<TextureLayout, Void>>(nameof(EmberTexture), TextureLayout.Uniform(TID.MissingTexture));

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
    }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        EmberTexture.Initialize(this);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        HasEmbers = builder.Define(nameof(HasEmbers)).Boolean().Attribute();
    }

    private void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        if (!message.State.Get(HasEmbers)) return;

        State cooled = message.State.With(HasEmbers, value: false);
        message.World.SetBlock(cooled, message.Position);
    }

    private Boolean IsAnimated(Boolean original, State state)
    {
        return state.Get(HasEmbers);
    }

    private TextureLayout GetTexture(TextureLayout original, State state)
    {
        return state.Get(HasEmbers) ? EmberTexture.Get() : original;
    }

    /// <summary>
    ///     Enable the ember state for the given state.
    /// </summary>
    /// <param name="state">The state to modify.</param>
    /// <returns>The modified state.</returns>
    public State WithEmbers(State state)
    {
        state.Set(HasEmbers, value: true);

        return state;
    }
}
