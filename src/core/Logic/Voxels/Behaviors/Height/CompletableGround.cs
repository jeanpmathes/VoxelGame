// <copyright file="CompletableGround.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Contents;
using VoxelGame.Toolkit.Utilities;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Height;

/// <summary>
///     A <see cref="PartialHeight" /> block which can be completed to serve as full ground for another block.
///     Other blocks can then for example use this when being placed and expecting full ground to be present.
/// </summary>
public partial class CompletableGround : BlockBehavior, IBehavior<CompletableGround, BlockBehavior, Block>
{
    private Block replacement = null!;

    [Constructible]
    private CompletableGround(Block subject) : base(subject) {}

    /// <summary>
    ///     The block that will replace this block to complete it.
    /// </summary>
    public ResolvedProperty<CID?> Replacement { get; } = ResolvedProperty<CID?>.New<Exclusive<CID?, Void>>(nameof(Replacement));

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Replacement.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Replacement.Get() == null)
            validator.ReportWarning("Replacement block is not set");

        if (Replacement.Get() == Subject.ContentID)
            validator.ReportWarning("Replacement block cannot be the same as the block itself");

        replacement = Blocks.Instance.SafelyTranslateContentID(Replacement.Get());

        if (replacement == Blocks.Instance.Core.Error && Replacement.Get() != Blocks.Instance.Core.Error.ContentID)
            validator.ReportWarning($"The replacement block '{Replacement}' could not be found");

        if (!replacement.IsFullySolid(replacement.States.Default))
            validator.ReportWarning("Replacement block is not fully solid");
    }

    /// <summary>
    ///     Make this block into a complete, solid block.
    /// </summary>
    /// <param name="world">The world in which the operation takes place.</param>
    /// <param name="position">The position of the block.</param>
    public void BecomeComplete(World world, Vector3i position)
    {
        if (replacement == Blocks.Instance.Core.Error)
            return;

        world.SetBlock(replacement.States.Default, position);
    }
}
