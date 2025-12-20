// <copyright file="Paintable.cs" company="VoxelGame">
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
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Visuals;
using VoxelGame.Core.Visuals;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Materials;

/// <summary>
///     Blocks that can be painted with different colors.
/// </summary>
public partial class Paintable : BlockBehavior, IBehavior<Paintable, BlockBehavior, Block>
{
    [Constructible]
    private Paintable(Block subject) : base(subject)
    {
        subject.Require<Meshed>().Tint.ContributeFunction(GetTint);
    }

    [LateInitialization] private partial IAttributeData<ColorS> Color { get; set; }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorInteractionMessage>(OnActorInteract);
    }

    /// <inheritdoc />
    public override void DefineState(IStateBuilder builder)
    {
        Color = builder.Define(nameof(Color)).List(ColorS.NamedColors, ColorS.GetNameOfNamedColorByIndex).Attribute();
    }

    private ColorS GetTint(ColorS original, State state)
    {
        return state.Get(Color);
    }

    private void OnActorInteract(Block.IActorInteractionMessage message)
    {
        Int32 currentIndex = message.State.GetValueIndex(Color);
        Int32 nextIndex = (currentIndex + 1) % ColorS.NamedColors.Count;

        State newState = message.State.With(Color, ColorS.GetNamedColorByIndex(nextIndex));

        message.Actor.World.SetBlock(newState, message.Position);
    }
}
