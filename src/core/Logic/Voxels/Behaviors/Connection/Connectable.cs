// <copyright file="Connectable.cs" company="VoxelGame">
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
using VoxelGame.Core.Behaviors.Aspects;
using VoxelGame.Core.Behaviors.Aspects.Strategies;
using VoxelGame.Core.Logic.Attributes;
using Void = VoxelGame.Toolkit.Utilities.Void;

namespace VoxelGame.Core.Logic.Voxels.Behaviors.Connection;

/// <summary>
///     Allows blocks like fences, walls and such to connect to this block.
///     Connectivity may only occur on the lateral sides of the block.
/// </summary>
public partial class Connectable : BlockBehavior, IBehavior<Connectable, BlockBehavior, Block>
{
    /// <summary>
    ///     The connection strength of this block.
    /// </summary>
    [Flags]
    public enum Strengths
    {
        /// <summary>
        ///     The block does not allow any connections.
        /// </summary>
        None = 0,

        /// <summary>
        ///     The block allows thin connectable blocks.
        ///     Thin connectable blocks are at least two sixteenth wide.
        /// </summary>
        Thin = 1 << 0,

        /// <summary>
        ///     The block allows wide connectable blocks.
        ///     Wide connectable blocks are at least four sixteenth wide.
        ///     Note that the connecting block might require this block to close of the connection surface.
        /// </summary>
        Wide = 1 << 1,

        /// <summary>
        ///     The block allows all connectable blocks.
        ///     Blocks which are full and solid will typically use this.
        /// </summary>
        All = Thin | Wide
    }

    [Constructible]
    private Connectable(Block subject) : base(subject)
    {
        IsConnectionAllowed = Aspect<Boolean, (Side, State)>.New<LogicalAnd<(Side, State)>>(nameof(IsConnectionAllowed), this);
    }

    /// <summary>
    ///     The strength of the connection of this block.
    /// </summary>
    public ResolvedProperty<Strengths> Strength { get; } = ResolvedProperty<Strengths>.New<Masking<Strengths, Void>>(nameof(Strength), Strengths.All);

    /// <summary>
    ///     Whether connection to this block is allowed in the given state.
    /// </summary>
    public Aspect<Boolean, (Side side, State state)> IsConnectionAllowed { get; }

    /// <inheritdoc />
    public override void OnInitialize(BlockProperties properties)
    {
        Strength.Initialize(this);
    }

    /// <inheritdoc />
    protected override void OnValidate(IValidator validator)
    {
        if (Strength.Get() == Strengths.None)
            validator.ReportWarning("Connectable blocks should have at least one connection strength defined");
    }

    /// <summary>
    ///     Check whether another connectable can connect to this one on the given side.
    /// </summary>
    /// <param name="state">The state of this block.</param>
    /// <param name="other">The other connectable to check connection against.</param>
    /// <param name="side">The side of this block to check connection on.</param>
    /// <returns><c>true</c> if the blocks can connect, <c>false</c> otherwise.</returns>
    public Boolean CanConnect(State state, Side side, Connectable other)
    {
        if (!IsConnectionAllowed.GetValue(original: true, (side, state))) return false;

        Strengths a = Strength.Get();
        Strengths b = other.Strength.Get();

        return (a & b) != Strengths.None;
    }
}
