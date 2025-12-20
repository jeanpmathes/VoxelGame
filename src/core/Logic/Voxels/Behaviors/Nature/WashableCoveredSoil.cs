// <copyright file="WashableCoveredSoil.cs" company="VoxelGame">
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

using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;
using VoxelGame.Core.Logic.Voxels.Behaviors.Fluids;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Nature;

/// <summary>
///     Extends <see cref="CoveredSoil" /> to remove the cover when becoming wet.
/// </summary>
public partial class WashableCoveredSoil : BlockBehavior, IBehavior<WashableCoveredSoil, BlockBehavior, Block>
{
    [Constructible]
    private WashableCoveredSoil(Block subject) : base(subject)
    {
        subject.Require<Wet>();
        subject.Require<CoveredSoil>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Wet.IBecomeWetMessage>(OnBecomeWet);
    }

    private static void OnBecomeWet(Wet.IBecomeWetMessage message)
    {
        CoveredSoil.RemoveCover(message.World, message.Position);
    }
}
