// <copyright file="Modifiable.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Attributes;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     Allows to modify the height by interacting with the block.
/// </summary>
public partial class Modifiable : BlockBehavior, IBehavior<Modifiable, BlockBehavior, Block>
{
    [Constructible]
    private Modifiable(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<IModifyHeightMessage> ModifyHeight { get; set; }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        ModifyHeight = registry.RegisterEvent<IModifyHeightMessage>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IActorInteractionMessage>(OnActorInteractions);
    }

    private void OnActorInteractions(Block.IActorInteractionMessage message)
    {
        if (!ModifyHeight.HasSubscribers) return;

        ModifyHeightMessage modifyHeight = IEventMessage<ModifyHeightMessage>.Pool.Get();

        modifyHeight.World = message.Actor.World;
        modifyHeight.Position = message.Position;
        modifyHeight.State = message.State;

        ModifyHeight.Publish(modifyHeight);

        IEventMessage<ModifyHeightMessage>.Pool.Return(modifyHeight);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!ModifyHeight.HasSubscribers)
            validator.ReportWarning("Modifiable behavior has no effect as there are no subscribers");
    }

    /// <summary>
    ///     Sent when the height of the block should be modified.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IModifyHeightMessage
    {
        /// <summary>
        ///     The world in which the block is located.
        /// </summary>
        World World { get; }

        /// <summary>
        ///     The position of the block in the world.
        /// </summary>
        Vector3i Position { get; }

        /// <summary>
        ///     The current state of the block.
        /// </summary>
        State State { get; }
    }
}
