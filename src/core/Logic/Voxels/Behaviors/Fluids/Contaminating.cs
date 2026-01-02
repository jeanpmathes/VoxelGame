// <copyright file="Contaminating.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

/// <summary>
///     Contaminates water when the block is destroyed.
/// </summary>
public partial class Contaminating : BlockBehavior, IBehavior<Contaminating, BlockBehavior, Block>
{
    [Constructible]
    private Contaminating(Block subject) : base(subject)
    {
        subject.Require<Fillable>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IDestructionCompletedMessage>(OnDestructionCompleted);
    }

    private static void OnDestructionCompleted(Block.IDestructionCompletedMessage message)
    {
        Content? content = message.World.GetContent(message.Position);

        if (content is {Fluid: var fluid} && fluid.Fluid == Voxels.Fluids.Instance.FreshWater)
            message.World.SetFluid(
                Voxels.Fluids.Instance.WasteWater.AsInstance(fluid.Level, fluid.IsStatic),
                message.Position);
    }
}
