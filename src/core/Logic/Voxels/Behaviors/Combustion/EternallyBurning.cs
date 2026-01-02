// <copyright file="EternallyBurning.cs" company="VoxelGame">
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

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;

/// <summary>
///     Does not stop burning.
/// </summary>
public partial class EternallyBurning : BlockBehavior, IBehavior<EternallyBurning, BlockBehavior, Block>
{
    [Constructible]
    private EternallyBurning(Block subject) : base(subject)
    {
        subject.Require<Combustible>();
    }

    /// <inheritdoc />
    public override void SubscribeToEvents(IEventBus bus)
    {
        bus.Subscribe<Combustible.IBurnMessage>(OnBurn);
    }

    private static void OnBurn(Combustible.IBurnMessage message)
    {
        // Nothing to do, subscription just prevents fall-back behavior.
    }
}
