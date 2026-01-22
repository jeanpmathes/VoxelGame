// <copyright file="Soil.cs" company="VoxelGame">
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
using VoxelGame.Core.Logic.Attributes;
using VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;
using VoxelGame.Core.Utilities.Units;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Core behavior for all soil blocks.
///     Not only the soil block itself, but also blocks that contain significant amounts of soil.
/// </summary>
public partial class Soil : BlockBehavior, IBehavior<Soil, BlockBehavior, Block>
{
    [Constructible]
    private Soil(Block subject) : base(subject)
    {
        subject.Require<Regolith>();
        subject.Require<Plantable>();
        subject.Require<Membrane>().MaxViscosity.Initializer.ContributeConstant(new Viscosity {MilliPascalSeconds = 6.5});
        subject.Require<Fillable>().IsFluidMeshed.Initializer.ContributeConstant(value: false);
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Block.IRandomUpdateMessage>(OnRandomUpdate);
        bus.Subscribe<Block.IGeneratorUpdateMessage>(OnGeneratorUpdate);

        bus.Subscribe<AshCoverable.IAshCoverMessage>(OnAshCover);
    }

    private static void OnRandomUpdate(Block.IRandomUpdateMessage message)
    {
        FluidInstance? fluid = message.World.GetFluid(message.Position);

        if (fluid is {IsAnyWater: true, Level.IsFull: true})
            message.World.SetContent(Content.Create(Blocks.Instance.Environment.Mud), message.Position);
    }

    private static void OnGeneratorUpdate(Block.IGeneratorUpdateMessage message)
    {
        if (message.Content.Fluid is {IsAnyWater: true, Level.IsFull: true})
            message.Content = Content.Create(Blocks.Instance.Environment.Mud);
    }

    private static void OnAshCover(AshCoverable.IAshCoverMessage message)
    {
        message.World.SetBlock(new State(Blocks.Instance.Environment.AshCoveredSoil), message.Position);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!Subject.Is<Wet>()) validator.ReportWarning("Soil blocks must be able to get wet in some way, preferably with visual representation of that");
    }
}
