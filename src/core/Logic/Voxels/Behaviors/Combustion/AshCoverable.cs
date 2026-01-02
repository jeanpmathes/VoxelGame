// <copyright file="AshCoverable.cs" company="VoxelGame">
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

using OpenTK.Mathematics;
using VoxelGame.Annotations.Attributes;
using VoxelGame.Core.Behaviors;
using VoxelGame.Core.Behaviors.Events;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Combustion;

/// <summary>
///     A block that is not <see cref="Combustible" /> but can be covered with ash if the block above it is burned.
/// </summary>
public partial class AshCoverable : BlockBehavior, IBehavior<AshCoverable, BlockBehavior, Block>
{
    [Constructible]
    private AshCoverable(Block subject) : base(subject) {}

    [LateInitialization] private partial IEvent<IAshCoverMessage> AshCover { get; set; }

    /// <inheritdoc />
    public override void DefineEvents(IEventRegistry registry)
    {
        AshCover = registry.RegisterEvent<IAshCoverMessage>();
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (!AshCover.HasSubscribers)
            validator.ReportWarning("No operation registered to cover the block with ash");

        if (Subject.Is<Combustible>())
            validator.ReportWarning("Combustible blocks should not be coverable with ash as they burn instead");
    }

    /// <summary>
    ///     Cover the block with ash.
    /// </summary>
    /// <param name="world">The world the block is in.</param>
    /// <param name="position">The position of the block to cover with ash.</param>
    public void CoverWithAsh(World world, Vector3i position)
    {
        if (!AshCover.HasSubscribers) return;

        AshCoverMessage ashCover = IEventMessage<AshCoverMessage>.Pool.Get();

        ashCover.World = world;
        ashCover.Position = position;

        AshCover.Publish(ashCover);

        IEventMessage<AshCoverMessage>.Pool.Return(ashCover);
    }

    /// <summary>
    ///     Sent when a block should be covered with ash.
    /// </summary>
    [GenerateRecord(typeof(IEventMessage<>))]
    public interface IAshCoverMessage
    {
        /// <summary>
        ///     The world the block is in.
        /// </summary>
        World World { get; }

        /// <summary>
        ///     The position of the block to cover with ash.
        /// </summary>
        Vector3i Position { get; }
    }
}
